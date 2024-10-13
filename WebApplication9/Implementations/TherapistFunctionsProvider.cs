using Database.Models;
using Database.RepositoryImplementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication9.Helpers;
using WebApplication9.Interfaces;
using Framework.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication9.Areas.Therapist.ViewModels;
using DataTransferObjects.ViewModels.Shared;
using System.Transactions;
using Framework.Providers;
using Stripe;
using DataTransferObjects.ViewModels.Client;

namespace WebApplication9.Implementations
{
    public class TherapistFunctionsProvider : ITherapistFunctionsProvider
    {
        private IErrorLogger _errors => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IErrorLogger>();
        private IDateTimeHelper _dateHelper => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IDateTimeHelper>();
        private readonly UnitOfWork _context;
        private ClaimsPrincipal User => new HttpContextAccessor().HttpContext.User;
        private UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        public TherapistFunctionsProvider()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public async Task<TherapistAccountCompletionStatus> TherapistHasCompletedAccountSetupAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return TherapistAccountCompletionStatus.Error;

                var therapist = _context.Therapists.GetById(user.TherapistAccountId);

                if (therapist == null)
                    return TherapistAccountCompletionStatus.Error;

                if (HasCompletedTherapistAccountSetup(therapist))
                    return TherapistAccountCompletionStatus.Completed;

                return TherapistAccountCompletionStatus.NotSetUpYet;
            }
            catch (Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "TherapistFunctionsProvider", "TherapistHasCompletedAccountSetupAsync");
                return TherapistAccountCompletionStatus.Error;
            }
        }

        public bool HasCompletedTherapistAccountSetup(Therapists therapist)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(therapist.About) || !HasContactMethods(therapist.Id))
                    return false;

                return true;
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "WebApplication9", "TherapistFunctionsProvider", "HasCompletedTherapistAccountSetup");
                return false;
            }
        }

        public async Task<bool> CreateTherapistAsync(Therapists therapist, string userIdToAttachTherapistTo)
        {
            try
            {
                if (therapist == null)
                    return false;

                if (string.IsNullOrWhiteSpace(therapist.Id) || string.IsNullOrWhiteSpace(userIdToAttachTherapistTo))
                    return false;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var user = await _userManager.FindByIdAsync(userIdToAttachTherapistTo);

                        if (user == null) throw new Exception($"Unable to load user '{userIdToAttachTherapistTo}'.");

                        using (var scope2 = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                var removedFromClientRole = await _userManager.RemoveFromRoleAsync(user, UserRoles.Client);
                                var addedToTherapistRole = await _userManager.AddToRoleAsync(user, UserRoles.Therapist);

                                if (!removedFromClientRole.Succeeded || !addedToTherapistRole.Succeeded)
                                {
                                    var errorMessage = string.Empty;

                                    if (removedFromClientRole.Errors.Any())
                                        errorMessage = string.Join('|', removedFromClientRole.Errors.Select(e => e.Description));

                                    if (addedToTherapistRole.Errors.Any())
                                        errorMessage += string.Join('|', addedToTherapistRole.Errors.Select(e => e.Description));

                                    throw new Exception(errorMessage);
                                }

                                _context.Therapists.Insert(therapist);
                                user.TherapistAccountId = therapist.Id;

                                await _context.SaveAsync();

                                scope2.Complete();
                            }
                            catch (Exception)
                            {
                                scope2.Dispose();
                                throw;
                            }

                            using (var scope3 = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    var updated = await _userManager.UpdateAsync(user);

                                    if (!updated.Succeeded)
                                        throw new Exception(string.Join('|', updated.Errors.Select(e => e.Description)));

                                    scope3.Complete();
                                }
                                catch (Exception)
                                {
                                    scope3.Dispose();
                                    throw;
                                }
                            }

                            //If it makes problem here in the future,
                            //uncomment code below and comment this code
                            scope.Complete();
                            return true;
                        }

                        //scope.Complete();
                        //return true;
                    }
                    catch (Exception)
                    {
                        scope.Dispose();
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "TherapistFunctionsProvider", "CreateTherapistAsync");
                return false;
            }
        }

        public List<PsychotherapyTechniques> GetPsychotherapyTechniques(string therapistId)
        {
            if (string.IsNullOrEmpty(therapistId))
                return new List<PsychotherapyTechniques>();

            return (from thTech in _context.TherapistPsychotherapyTechniques.ReadOnlyFind(p => p.TherapistId == therapistId)
                    join tech in _context.PsychotherapyTechniques.ReadOnlyGetAll()
                    on thTech.PsychotherapyTechniqueId equals tech.Id
                    select tech).ToList();
        }

        public List<Specialities> GetSpecialties(string therapistId)
        {
            if (string.IsNullOrEmpty(therapistId))
                return new List<Specialities>();

            return (from thSpec in _context.TherapistsSpecialities.ReadOnlyFind(s => s.TherapistId == therapistId)
                    join spec in _context.Specialities.ReadOnlyGetAll()
                    on thSpec.SpecialityId equals spec.Id
                    select spec).ToList();
        }

        public List<string> GetPsychotherapyTechniqueNames(string therapistId)
        {
            var techniqueNames = GetPsychotherapyTechniques(therapistId)
                                    .Select(s => s.Name)
                                    .ToList();

            return techniqueNames.Any() ? techniqueNames : new List<string>();
        }

        public List<string> GetSpecialtyNames(string therapistId)
        {
            var specialtyNames = GetSpecialties(therapistId)
                                    .Select(s => s.Name)
                                    .ToList();

            return specialtyNames.Any() ? specialtyNames : new List<string>();
        }

        public string GetPsychotherapyTechniquesNamesForDisplay(string therapistId, string delimiter = " · ")
        {
            var psyTechNames = GetPsychotherapyTechniqueNames(therapistId);
            return psyTechNames.Any() ? string.Join(" · ", psyTechNames) : string.Empty;
        }

        public string GetSpecialtiesNamesForDisplay(string therapistId, string delimiter = " · ")
        {
            var specialtyNames = GetSpecialtyNames(therapistId);
            return specialtyNames.Any() ? string.Join(" · ", specialtyNames) : string.Empty;
        }

        public bool HasContactMethods(string therapistId)
        {
            return _context.TherapistsContactMethods.ReadOnlyAny(cm => cm.TherapistId == therapistId);
        }

        public bool HasContactMethod(string therapistId, string contactMethodId)
        {
            if (string.IsNullOrWhiteSpace(therapistId) || string.IsNullOrWhiteSpace(contactMethodId))
                return false;

            return _context.TherapistsContactMethods.ReadOnlyAny(i => i.TherapistId == therapistId &&
                                                               i.ContactMethodId == contactMethodId);
        }

        public List<ContactMethods> GetContactMethods(string therapistId)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return new List<ContactMethods>();

            return (from thCm in _context.TherapistsContactMethods.Find(c => c.TherapistId == therapistId)
                    join cm in _context.ContactMethods.GetAll()
                    on thCm.ContactMethodId equals cm.Id
                    select cm).ToList();
        }

        public List<ContactMethodViewModel> GetContactMethodsViewModel(string therapistId)
        {
            var contactsVm = new List<ContactMethodViewModel>();

            foreach (var contact in GetContactMethods(therapistId))
            {
                contactsVm.Add(new ContactMethodViewModel
                {
                    Name = contact.Name,
                    Color = contact.Color,
                    Icon = contact.Icon
                });
            }

            return contactsVm;
        }

        public List<Consultations> GetUpcomingConsultations(string therapistId, bool orderByStartDateTime)
        {
            var todaysDate = DateTime.UtcNow.Date;
            var upcomingConsultations = _context.Consultations.Find(c => c.TherapistId == therapistId)
                                                              .Where(c => c.StartDateTime.Date > todaysDate);

            return orderByStartDateTime ? upcomingConsultations.OrderBy(c => c.StartDateTime).ToList() : upcomingConsultations.ToList();
        }

        public List<Sessions> GetUpcomingSessions(string therapistId, bool orderByStartDateTime)
        {
            var todaysDate = DateTime.UtcNow.Date;
            var upcomingSessions = _context.Sessions.Find(s => s.TherapistId == therapistId)
                                                    .Where(s => s.StartDateTime.Date > todaysDate);

            return orderByStartDateTime ? upcomingSessions.OrderBy(s => s.StartDateTime).ToList() : upcomingSessions.ToList();
        }

        public List<ConsultationViewModel> GetUpcomingConsultationsViewModels(string therapistId)
        {
            var consVms = new List<ConsultationViewModel>();

            foreach (var cons in GetUpcomingConsultations(therapistId, orderByStartDateTime: true))
            {
                consVms.Add(new ConsultationViewModel
                {
                    ConsultationId = cons.Id,
                    StartDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(cons.StartDateTime),
                    EndDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(cons.EndDateTime),
                    Booked = cons.Booked
                });
            }

            return consVms;
        }

        public List<SessionViewModel> GetUpcomingSessionsViewModels(string therapistId)
        {
            var sessVms = new List<SessionViewModel>();

            foreach (var sess in GetUpcomingSessions(therapistId, orderByStartDateTime: true))
            {
                sessVms.Add(new SessionViewModel
                {
                    SessionId = sess.Id,
                    Subject = sess.Subject,
                    //Description = sess.Description,
                    Price = sess.Price,
                    Type = sess.Type,
                    StartDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(sess.StartDateTime),
                    EndDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(sess.EndDateTime),
                    Booked = sess.Booked
                });
            }

            return sessVms;
        }

        public async Task<bool> HasAnyUpcomingSessionAsync(string therapistId)
        {
            var minDate = DateTime.UtcNow.AddHours(24);
            return await _context.Sessions.ReadOnlyAnyAsync(s => s.TherapistId == therapistId && s.Booked == 0 && minDate <= s.StartDateTime);
        }

        public async Task<bool> HasAnyUpcomingConsultationAsync(string therapistId)
        {
            var minDate = DateTime.UtcNow.AddHours(24);
            return await _context.Consultations.ReadOnlyAnyAsync(c => c.TherapistId == therapistId && c.Booked == 0 && minDate <= c.StartDateTime);
        }

        public double GetTherapistAverageRating(string therapistId)
        {
            if (string.IsNullOrEmpty(therapistId))
                return 0;

            var ratings = _context.Ratings.ReadOnlyFind(r => r.TherapistId == therapistId).ToList();

            return ratings.Any() ? ratings.Average(r => r.Rating) : 0;
        }

        public List<PartialViewModels.ClientReviewPartialViewModel> GetClientReviews(string therapistId)
        {
            if (string.IsNullOrEmpty(therapistId))
                return new List<PartialViewModels.ClientReviewPartialViewModel>();

            var clientReviews = new List<PartialViewModels.ClientReviewPartialViewModel>();

            foreach (var rating in _context.Ratings.ReadOnlyFind(r => r.TherapistId == therapistId).OrderByDescending(r => r.RatingDate).ToList())
            {
                var tempItem = new PartialViewModels.ClientReviewPartialViewModel();
                tempItem.Map(rating);
                tempItem.RatingDate = rating.RatingDate.HasValue ? _dateHelper.ConvertDateTimeFromUtcToLocalDateString(rating.RatingDate.Value.Date) : null;
                clientReviews.Add(tempItem);
            }

            return clientReviews;
        }

        public int GetNumberOfBookedSessions(string therapistId)
        {
            if (string.IsNullOrEmpty(therapistId))
                return 0;

            return _context.BookedSessions.Find(b => b.TherapistId == therapistId).Count();
        }

        public int GetNumberOfUniqueClients(string therapistId)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return 0;

            var bookedSessions = _context.BookedSessions.Find(b => b.TherapistId == therapistId).ToList();

            if (bookedSessions.Count == 0)
                return 0;

            return bookedSessions.GroupBy(b => b.ClientId).ToList().Count;
        }

        public List<Therapists> GetTherapists(string filter, string predicate)
        {
            var therapists = _context.Therapists.GetAll();

            if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(predicate))
                return therapists.ToList();

            switch (filter.ToLower())
            {
                case "najzakazivaniji":
                    {
                        DateTime fromDate = new DateTime();
                        DateTime toDate = DateTime.UtcNow;

                        switch (predicate)
                        {
                            case "7":
                                {
                                    fromDate = toDate.Subtract(new TimeSpan(7, 0, 0, 0));
                                    break;
                                }
                            case "14":
                                {
                                    fromDate = toDate.Subtract(new TimeSpan(14, 0, 0, 0));
                                    break;
                                }
                            case "30":
                                {
                                    fromDate = toDate.AddMonths(-1);
                                    break;
                                }
                            case "60":
                                {
                                    fromDate = toDate.AddMonths(-2);
                                    break;
                                }
                            case "180":
                                {
                                    fromDate = toDate.AddMonths(-6);
                                    break;
                                }
                            case "365":
                                {
                                    fromDate = toDate.AddYears(-1);
                                    break;
                                }
                        }

                        var mostPopularTherapists = new List<Therapists>();
                        var bookedSessionsTherapistIds = _context.BookedSessions.Find(s => s.StartTime >= fromDate).GroupBy(s => s.TherapistId).OrderByDescending(g => g.Count()).ToList().Select(s => s.Key);
                        foreach (var therapistId in bookedSessionsTherapistIds)
                        {
                            var therapist = _context.Therapists.GetById(therapistId);
                            if (therapist == null) continue;
                            mostPopularTherapists.Add(therapist);
                        }

                        return mostPopularTherapists.Count > 0 ? mostPopularTherapists : therapists.ToList();
                    }

                case "specijalnost":
                    {
                        return (from spec in _context.Specialities.Find(s => s.Id.ToLower() == predicate.ToLower())
                                join thSpec in _context.TherapistsSpecialities.GetAll()
                                on spec.Id equals thSpec.SpecialityId
                                join th in therapists
                                on thSpec.TherapistId equals th.Id
                                select th).ToList();
                    }

                case "psihoterapijska-tehnika":
                    {
                        return (from psyTech in _context.PsychotherapyTechniques.Find(p => p.Id.ToLower() == predicate.ToLower())
                                join thPsyTech in _context.TherapistPsychotherapyTechniques.GetAll()
                                on psyTech.Id equals thPsyTech.PsychotherapyTechniqueId
                                join th in therapists
                                on thPsyTech.TherapistId equals th.Id
                                select th).ToList();
                    }

                case "ocena":
                    {
                        //var thRt = from th in _context.Therapists.GetAll()
                        //         join rt in _context.Ratings.GetAll()
                        //         on th.Id equals rt.TherapistId into therapistsWithRatings
                        //         from rt in therapistsWithRatings.DefaultIfEmpty()
                        //         select new TherapistRatingLinqModel
                        //         {
                        //             TherapistId = th.Id,
                        //             Rating = rt == null ? 0 : rt.Rating
                        //         };

                        //var d = thRt.GroupBy(r => r.TherapistId).ToList();

                        //foreach(var q in d)
                        //{
                        //    q.Average(r => r.Rating)
                        //}

                        var therapistsRatings = new List<TherapistRatingLinqModel>();

                        foreach (var thr in therapists)
                        {
                            var ratings = _context.Ratings.Find(r => r.TherapistId == thr.Id).ToList();

                            therapistsRatings.Add(new TherapistRatingLinqModel
                            {
                                Therapist = thr,
                                Rating = ratings.Count > 0 ? ratings.Average(r => r.Rating) : 0
                            });
                        }

                        //var therapistsOrdered = new List<Therapists>();

                        if (predicate.ToLower() == "više-ka-niže") return therapistsRatings.OrderByDescending(th => th.Rating).Select(th => th.Therapist).ToList();
                        else if (predicate.ToLower() == "niže-ka-više") return therapistsRatings.OrderBy(th => th.Rating).Select(th => th.Therapist).ToList();

                        return therapists.ToList();
                    }

                case "cena":
                    {
                        var therapistsPrices = new List<TherapistSessionPriceLinqModel>();

                        foreach (var thr in therapists)
                        {
                            if (GetUpcomingSessions(thr.Id, orderByStartDateTime: false).Any())
                            {
                                var prices = GetPriceRange(thr.Id);

                                therapistsPrices.Add(new TherapistSessionPriceLinqModel
                                {
                                    Therapist = thr,
                                    MinSessionPrice = prices[0]
                                });
                            }
                        }

                        if (predicate.ToLower() == "više-ka-niže") return therapistsPrices.OrderByDescending(th => th.MinSessionPrice).Select(th => th.Therapist).ToList();
                        else if (predicate.ToLower() == "niže-ka-više") return therapistsPrices.OrderBy(th => th.MinSessionPrice).Select(th => th.Therapist).ToList();

                        return therapists.ToList();
                    }

                default: return therapists.ToList();
            }
        }

        public List<Therapists> GetTherapistsWithSetUpAccount(string filter = null, string predicate = null)
        {
            return GetTherapists(filter, predicate).Where(th => !string.IsNullOrWhiteSpace(th.About) &&
                                                                HasContactMethods(th.Id)).ToList();
        }

        public Therapists GetTherapistWithSetUpAccount(string therapisId)
        {
            try
            {
                var therapist = _context.Therapists.GetById(therapisId);

                if (string.IsNullOrWhiteSpace(therapist.About) || !HasContactMethods(therapisId))
                    return null;

                return therapist;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task DeleteTherapistContactMethods(string therapistId)
        {
            foreach (var method in _context.TherapistsContactMethods.Find(contact => contact.TherapistId == therapistId).ToList())
                _context.TherapistsContactMethods.Delete(method);

            await _context.SaveAsync();
        }

        public List<PsychotherapyTechniqueExperienceViewModel> GetPsychotherapyTechniquesExperiences(string therapistId)
        {

            return (from thPsyTech in _context.TherapistPsychotherapyTechniques.Find(tech => tech.TherapistId == therapistId)
                    join psyTech in _context.PsychotherapyTechniques.GetAll()
                    on thPsyTech.PsychotherapyTechniqueId equals psyTech.Id
                    select new PsychotherapyTechniqueExperienceViewModel
                    {
                        Id = psyTech.Id,
                        Name = psyTech.Name,
                        Color = psyTech.Color,
                        Icon = psyTech.Icon,
                        FromDate = thPsyTech.FromDate.HasValue ? (DateTime?)_dateHelper.ConvertDateTimeFromUtcToLocal(thPsyTech.FromDate.Value) : null,
                        ToDate = thPsyTech.ToDate.HasValue ? (DateTime?)_dateHelper.ConvertDateTimeFromUtcToLocal(thPsyTech.ToDate.Value) : null,
                        Present = thPsyTech.Present.GetValueOrDefault(),
                        City = thPsyTech.City,
                        Country = thPsyTech.Country,
                        Description = thPsyTech.Description
                    }).ToList();
        }

        public List<PsychotherapyTechniqueExperienceProfileViewModel> GetPsychotherapyTechniquesExperiencesForProfile(string therapistId)
        {
            //return (from thPsyTech in _context.TherapistPsychotherapyTechniques.Find(tech => tech.TherapistId == therapistId)
            //        join psyTech in _context.PsychotherapyTechniques.GetAll()
            //        on thPsyTech.PsychotherapyTechniqueId equals psyTech.Id
            //        select new PsychotherapyTechniqueExperienceProfileViewModel
            //        {
            //            Name = psyTech.Name,
            //            Color = psyTech.Color,
            //            Icon = psyTech.Icon,
            //            FromDate = thPsyTech.FromDate.HasValue ? _dateHelper.ConvertDateTimeFromUtcToLocalDateString(thPsyTech.FromDate.Value) : string.Empty,
            //            ToDate = thPsyTech.Present.GetValueOrDefault() ? "Present" : _dateHelper.ConvertDateTimeFromUtcToLocalDateString(thPsyTech.ToDate.GetValueOrDefault()),
            //            DateDifference = thPsyTech.Present.GetValueOrDefault() ? _dateHelper.ConvertTimeFromUtcToLocalString(new DateTime() + (DateTime.UtcNow.Date - thPsyTech.FromDate.GetValueOrDefault().Date)) :
            //                                                                     _dateHelper.ConvertTimeFromUtcToLocalString(new DateTime() + (thPsyTech.ToDate.GetValueOrDefault().Date - thPsyTech.FromDate.GetValueOrDefault().Date)),
            //            City = thPsyTech.City,
            //            Country = thPsyTech.Country,
            //            Description = thPsyTech.Description
            //        }).ToList();

            var psyTechExps = new List<PsychotherapyTechniqueExperienceProfileViewModel>();

            foreach (var thPsyTech in _context.TherapistPsychotherapyTechniques.ReadOnlyFind(tech => tech.TherapistId == therapistId).ToList())
            {
                var psyTech = _context.PsychotherapyTechniques.ReadOnlyFind(tech => tech.Id == thPsyTech.PsychotherapyTechniqueId && thPsyTech.TherapistId == therapistId)
                                                              .SingleOrDefault();

                if (psyTech != default)
                {
                    var experience = new PsychotherapyTechniqueExperienceProfileViewModel
                    {
                        Id = psyTech.Id,
                        Name = psyTech.Name,
                        Color = psyTech.Color,
                        Icon = psyTech.Icon,
                        Present = thPsyTech.Present.GetValueOrDefault(),
                        City = !string.IsNullOrWhiteSpace(thPsyTech.City) ? thPsyTech.City + ", " : string.Empty,
                        Country = !string.IsNullOrWhiteSpace(thPsyTech.Country) ? thPsyTech.Country : string.Empty,
                        Description = !string.IsNullOrWhiteSpace(thPsyTech.Description) ? "“" + thPsyTech.Description + "”" : string.Empty
                    };

                    if (thPsyTech.FromDate.HasValue)
                        experience.FromDate = _dateHelper.ConvertDateTimeFromUtcToLocal(thPsyTech.FromDate.Value);
                    else experience.FromDate = null;

                    if (((thPsyTech.Present.HasValue && !thPsyTech.Present.Value) || !thPsyTech.Present.HasValue) && thPsyTech.ToDate.HasValue)
                        experience.ToDate = _dateHelper.ConvertDateTimeFromUtcToLocal(thPsyTech.ToDate.Value);
                    else experience.ToDate = null;

                    psyTechExps.Add(experience);
                }
            }

            return psyTechExps;
        }

        public List<SpecialtyExperienceViewModel> GetSpecialtiesExperiences(string therapistId)
        {
            return (from thSpec in _context.TherapistsSpecialities.Find(spec => spec.TherapistId == therapistId)
                    join spec in _context.Specialities.GetAll()
                    on thSpec.SpecialityId equals spec.Id
                    select new SpecialtyExperienceViewModel
                    {
                        Id = spec.Id,
                        Name = spec.Name,
                        Color = spec.Color,
                        Icon = spec.Icon,
                        FromDate = thSpec.FromDate.HasValue ? (DateTime?)_dateHelper.ConvertDateTimeFromUtcToLocal(thSpec.FromDate.Value) : null,
                        ToDate = thSpec.ToDate.HasValue ? (DateTime?)_dateHelper.ConvertDateTimeFromUtcToLocal(thSpec.ToDate.Value) : null,
                        Present = thSpec.Present.GetValueOrDefault(),
                        City = thSpec.City,
                        Country = thSpec.Country,
                        Description = thSpec.Description
                    }).ToList();
        }

        public List<SpecialtyExperienceProfileViewModel> GetSpecialtiesExperiencesForProfile(string therapistId)
        {
            var specExps = new List<SpecialtyExperienceProfileViewModel>();

            foreach (var thSpec in _context.TherapistsSpecialities.ReadOnlyFind(spec => spec.TherapistId == therapistId).ToList())
            {
                var spec = _context.Specialities.ReadOnlyFind(s => s.Id == thSpec.SpecialityId && thSpec.TherapistId == therapistId)
                                                .SingleOrDefault();

                if (spec != default)
                {
                    var experience = new SpecialtyExperienceProfileViewModel
                    {
                        Id = spec.Id,
                        Name = spec.Name,
                        Color = spec.Color,
                        Icon = spec.Icon,
                        Present = thSpec.Present.GetValueOrDefault(),
                        City = !string.IsNullOrWhiteSpace(thSpec.City) ? thSpec.City + ", " : string.Empty,
                        Country = !string.IsNullOrWhiteSpace(thSpec.Country) ? thSpec.Country : string.Empty,
                        Description = !string.IsNullOrWhiteSpace(thSpec.Description) ? "“" + thSpec.Description + "”" : string.Empty
                    };

                    if (thSpec.FromDate.HasValue)
                        experience.FromDate = _dateHelper.ConvertDateTimeFromUtcToLocal(thSpec.FromDate.Value);
                    else experience.FromDate = null;

                    if (((thSpec.Present.HasValue && !thSpec.Present.Value) || !thSpec.Present.HasValue) && thSpec.ToDate.HasValue)
                        experience.ToDate = _dateHelper.ConvertDateTimeFromUtcToLocal(thSpec.ToDate.Value);
                    else experience.ToDate = null;

                    specExps.Add(experience);
                }
            }

            return specExps;
        }

        public List<SelectListItem> Get_ToChooseFrom_ContactMethods(string therapistId)
        {
            var items = new List<SelectListItem>();

            foreach (var contact in GetContactMethods(therapistId))
            {
                items.Add(new SelectListItem
                {
                    // When localizer is used, it will be like this:
                    // Text = contact.Icon + "|" + localizer[contact.name] + "|" + contact.Color + "|" + contact.Name,
                    Text = contact.Icon + "|" + contact.Name + "|" + contact.Color + "|" + contact.Id,
                    Value = contact.Id
                });
            }

            return items;
        }

        public List<SelectListItem> Get_ToChooseFrom_Topics()
        {
            var items = new List<SelectListItem>();

            foreach (var topic in _context.TherapistSupportTicketTopics.GetAll().ToList())
            {
                items.Add(new SelectListItem
                {
                    Value = topic.Id,
                    Text = topic.Icon + "|" + topic.Name + "|" + topic.Color
                });
            }

            return items;
        }

        public GenderViewModel GetGender(string therapistId)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return new GenderViewModel("Male");

            var therapist = _context.Therapists.GetById(therapistId);

            return new GenderViewModel((therapist != null && !string.IsNullOrWhiteSpace(therapist.Gender)) ? therapist.Gender : "Male");
        }

        public List<TherapistShowcaseViewModel> GetTherapistsWithUpcomingConsultations()
        {
            var minDate = DateTime.UtcNow.AddHours(24);

            var therapistShowcases = new List<TherapistShowcaseViewModel>();

            foreach (var therapistId in (from consultation in _context.Consultations.ReadOnlyFind(c => minDate < c.StartDateTime && c.Booked == 0)
                                         select consultation.TherapistId).Distinct().ToList())
            {
                var therapistUser = _context.AspNetUsers.ReadOnlyFind(usr => usr.TherapistAccountId == therapistId).SingleOrDefault();
                if (therapistUser != default)
                {
                    therapistShowcases.Add(new TherapistShowcaseViewModel
                    {
                        Id = therapistId,
                        FirstName = therapistUser.FirstName,
                        LastName = therapistUser.LastName,
                        ProfilePhotoPath = therapistUser.ProfilePhoto
                    });
                }
            }

            return therapistShowcases;
        }

        // DO NOT CHANGE THIS METHOD, ENTIRE SESSION BOOKING PAYMENT DEPENDS ON THIS METHOD
        public string GetPriceRangeString(string therapistId)
        {
            var prices = GetPriceRange(therapistId);

            if (!prices.Any()) return null;

            return prices.Count > 1 ? $"Cena seanse: {prices[0]} RSD - {prices[1]} RSD" : $"Cena seanse: {prices[0]} RSD";
        }

        // DO NOT CHANGE THIS METHOD, ENTIRE SESSION BOOKING PAYMENT DEPENDS ON THIS METHOD
        // This method does not exists in the Interface ITherapistFunctionsProvider, only local here.
        private List<double> GetPriceRange(string therapistId)
        {
            var sessions = _context.Sessions.ReadOnlyFind(s => s.TherapistId == therapistId)
                                            .Where(s => s.StartDateTime.Date > DateTime.UtcNow.Date);

            if (!sessions.Any()) return new List<double>();

            var min = sessions.Min(s => s.Price);
            var max = sessions.Max(s => s.Price);

            return min == max ? new List<double> { min } : new List<double> { min, max };
        }

        private class TherapistRatingLinqModel
        {
            public Therapists Therapist { get; set; }
            public double Rating { get; set; }
        }

        private class TherapistSessionPriceLinqModel
        {
            public Therapists Therapist { get; set; }
            public double MinSessionPrice { get; set; }
        }
    }
}
