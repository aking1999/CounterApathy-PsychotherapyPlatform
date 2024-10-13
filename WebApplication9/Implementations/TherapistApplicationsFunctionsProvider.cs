using Database.Models;
using Database.RepositoryImplementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.ViewModels;
using WebApplication9.Interfaces;
using Framework.Interfaces;
using Framework.Implementations;
using System.IO;
using Framework.Helpers;
using System.Transactions;
using Microsoft.AspNetCore.Hosting;

namespace WebApplication9.Implementations
{
    public class TherapistApplicationsFunctionsProvider : ITherapistApplicationsFunctionsProvider
    {
        private readonly IErrorLogger _errors = new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IErrorLogger>();
        private readonly IFileRepository _files;
        private readonly ITherapistFunctionsProvider _therapistFunctions;
        private readonly ICustomClientFunctionsProvider _clientFunctions;
        private IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        private readonly UnitOfWork _context;
        private UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        public TherapistApplicationsFunctionsProvider()
        {
            _files = new FileRepository();
            _therapistFunctions = new TherapistFunctionsProvider();
            _context = new UnitOfWork(new LajsnaProbaContext());
            _clientFunctions = new CustomClientFunctionsProvider(new HttpContextAccessor());
        }

        public async Task<bool> InsertPsychotherapyTechniquesForTherapistFromApplicationAsync(string therapistId, string applicationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(therapistId) || string.IsNullOrWhiteSpace(applicationId))
                    return false;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var appTech = _context.TherapistApplicationsPsychotherapyTechniques.ReadOnlyFind(tech => tech.TherapistApplicationId == applicationId).ToList();

                        if (!appTech.Any())
                            throw new Exception($"User with application '{applicationId}' did not chose any psychotherapy technique.");

                        foreach (var thTech in appTech)
                        {
                            _context.TherapistPsychotherapyTechniques.Insert(new TherapistPsychotherapyTechniques
                            {
                                TherapistId = therapistId,
                                PsychotherapyTechniqueId = thTech.PsychotherapyTechniqueId
                            });
                        }

                        await _context.SaveAsync();

                        scope.Complete();

                        return true;
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
                await _errors.SaveErrorAsync(e, "WebApplication9", "TherapistApplicationsFunctionsProvider", "InsertPsychotherapyTechniquesForTherapistFromApplicationAsync");
                return false;
            }
        }

        public async Task<bool> InsertSpecialtiesForTherapistFromApplicationAsync(string therapistId, string applicationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(therapistId) || string.IsNullOrWhiteSpace(applicationId))
                    return false;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var appSpec = _context.TherapistApplicationsSpecialities.ReadOnlyFind(spec => spec.TherapistApplicationId == applicationId).ToList();

                        if (!appSpec.Any())
                            throw new Exception($"User with application '{applicationId}' did not chose any specialty.");

                        foreach (var thSpec in appSpec)
                        {
                            _context.TherapistsSpecialities.Insert(new TherapistsSpecialities
                            {
                                TherapistId = therapistId,
                                SpecialityId = thSpec.SpecialityId
                            });
                        }

                        await _context.SaveAsync();

                        scope.Complete();

                        return true;
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
                await _errors.SaveErrorAsync(e, "WebApplication9", "TherapistApplicationsFunctionsProvider", "InsertSpecialtiesForTherapistFromApplicationAsync");
                return false;
            }
        }

        public TherapistApplications GetApplication(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var app = _context.TherapistApplications.Find(thApp => thApp.UserId == userId);

            if (app.Count() == 1)
                return app.ElementAt(0);

            if (app.Count() > 1)
                return null;
            //throw new MultipleApplicationsFromSameUserException();

            return null;
        }

        public List<ApplicationsViewModel> GetApplications(string predicate)
        {
            var applications = _context.TherapistApplications.ReadOnlyGetAll().ToList();

            if (string.IsNullOrWhiteSpace(predicate))
                return (from application in applications
                        join user in _context.AspNetUsers.ReadOnlyGetAll()
                        on application.UserId equals user.Id
                        orderby application.ApplicationDate descending
                        select new ApplicationsViewModel
                        {
                            ApplicationId = application.Id,
                            UserId = user.Id,
                            ProfilePhoto = GetApplicationPhotoPath(user.Id, application.Id),
                            FirstName = application.FirstName,
                            LastName = application.LastName,
                            Email = user.Email,
                            ApplicationDate = application.ApplicationDate,
                            Accepted = application.Accepted
                        }).ToList();

            switch (predicate.ToLower())
            {
                case "prihvaćeno":
                    {
                        applications = applications.Where(app => app.Accepted == 1)
                                                   .OrderByDescending(app => app.ApplicationDate).ToList();

                        break;
                    }
                case "na-čekanju":
                    {
                        applications = applications.Where(app => app.Accepted == 0)
                                                   .OrderByDescending(app => app.ApplicationDate).ToList();

                        break;
                    }
                case "odbijeno":
                    {
                        applications = applications.Where(app => app.Accepted == -1)
                                                   .OrderByDescending(app => app.ApplicationDate).ToList();

                        break;
                    }
                default: break;
            }

            return (from application in applications
                    join user in _context.AspNetUsers.ReadOnlyGetAll()
                    on application.UserId equals user.Id
                    orderby application.ApplicationDate descending
                    select new ApplicationsViewModel
                    {
                        UserId = user.Id,
                        //ProfilePhoto = application.ProfilePhoto,
                        ProfilePhoto = GetApplicationPhotoPath(user.Id, application.Id),
                        FirstName = application.FirstName,
                        LastName = application.LastName,
                        Email = user.Email,
                        ApplicationDate = application.ApplicationDate,
                        Accepted = application.Accepted
                    }).ToList();
        }

        public List<Specialities> GetTherapistApplicationSpecialties(string userId)
        {
            return !string.IsNullOrWhiteSpace(userId) ? (from thApp in _context.TherapistApplications.ReadOnlyFind(app => app.UserId == userId)
                                                         join appSpec in _context.TherapistApplicationsSpecialities.ReadOnlyGetAll()
                                                         on thApp.Id equals appSpec.TherapistApplicationId
                                                         join spec in _context.Specialities.ReadOnlyGetAll()
                                                         on appSpec.SpecialityId equals spec.Id
                                                         orderby spec.Name
                                                         select spec).ToList() : new List<Specialities>();
        }

        public List<PsychotherapyTechniques> GetTherapistApplicationPsychotherapyTechniques(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<PsychotherapyTechniques>();

            return (from thApp in _context.TherapistApplications.ReadOnlyFind(app => app.UserId == userId)
                    join appPsyTech in _context.TherapistApplicationsPsychotherapyTechniques.ReadOnlyGetAll()
                    on thApp.Id equals appPsyTech.TherapistApplicationId
                    join psyTech in _context.PsychotherapyTechniques.ReadOnlyGetAll()
                    on appPsyTech.PsychotherapyTechniqueId equals psyTech.Id
                    orderby psyTech.Name
                    select psyTech).ToList();
        }

        public string GetApplicationPhotoPath(string userId, string applicationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(applicationId))
                    return _files.DefaultProfilePhotoPath;

                var apps = _context.TherapistApplications.ReadOnlyFind(app => app.UserId == userId && app.Id == applicationId);

                if (apps.Any())
                {
                    var profilePhoto = apps.Single().ProfilePhoto;
                    if (_files.UserImageExists(_environment, profilePhoto))
                        return Path.Combine(_files.UserImagesPath, profilePhoto);
                }

                return _files.DefaultProfilePhotoPath;
            }
            catch (Exception)
            {
                return _files.DefaultProfilePhotoPath;
            }
        }

        public async Task<bool> AcceptApplicationAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                var userThatApplied = await _userManager.FindByIdAsync(userId);

                if (userThatApplied == null)
                    return false;

                var application = GetApplication(userThatApplied.Id);

                if (application == null)
                    return false;

                // Application is not pending, already Accepted/Rejected.
                if (application.Accepted != 0)
                    return false;

                if (_clientFunctions.HasUpcomingConsultation(userThatApplied.Id))
                    return false;

                if (_clientFunctions.HasUpcomingSession(userThatApplied.Id))
                    return false;

                var therapistId = Helper.GenerateNumbersId();

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var therapistCreated = await _therapistFunctions.CreateTherapistAsync(new Therapists
                        {
                            Id = therapistId,
                            OnVacation = 0,
                            Gender = application.Gender,
                            Street = application.Street,
                            HouseNumber = application.HouseNumber,
                            City = application.City,
                            Country = application.Country,
                            PostalCode = application.PostalCode,
                            University = application.University,
                            PastCompanies = application.PastCompanies,
                            UnderSupervision = application.UnderSupervision
                        }, userIdToAttachTherapistTo: userThatApplied.Id);

                        var techniquesInserted = await InsertPsychotherapyTechniquesForTherapistFromApplicationAsync(therapistId, application.Id);
                        var specialtiesInserted = await InsertSpecialtiesForTherapistFromApplicationAsync(therapistId, application.Id);

                        userThatApplied.ProfilePhoto = application.ProfilePhoto;
                        userThatApplied.FirstName = application.FirstName;
                        userThatApplied.LastName = application.LastName;
                        userThatApplied.PhoneNumber = application.PhoneNumber;
                        userThatApplied.YearOfBirth = application.YearOfBirth;

                        if (therapistCreated && techniquesInserted && specialtiesInserted)
                        {
                            using (var scope2 = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    var updated = await _userManager.UpdateAsync(userThatApplied);

                                    if (!updated.Succeeded)
                                        throw new Exception(string.Join('|', updated.Errors.Select(e => e.Description)));

                                    scope2.Complete();
                                }
                                catch (Exception)
                                {
                                    scope2.Dispose();
                                    throw;
                                }
                            }

                            application.Accepted = 1;
                            await _context.SaveAsync();

                            scope.Complete();

                            return true;
                        }

                        var errorMessage = string.Empty;
                        if (!therapistCreated) errorMessage = "Therapist not created | ";
                        if (!techniquesInserted) errorMessage += "Psychotherapy techniques not inserted | ";
                        if (!specialtiesInserted) errorMessage += "Specialties not inserted";

                        throw new Exception(errorMessage);
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
                await _errors.SaveErrorAsync(e, "WebApplication9", "TherapistApplicationsFunctionsProvider", "AcceptApplicationAsync");
                return false;
            }
        }

        public async Task<bool> RejectApplicationAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                var userThatApplied = await _userManager.FindByIdAsync(userId);

                if (userThatApplied == null) throw new Exception($"Unable to load user with ID '{userId}'.");

                var application = GetApplication(userThatApplied.Id);

                if (application == null)
                    return false;

                // Application is not pending, already Accepted/Rejected.
                if (application.Accepted != 0)
                    return false;

                application.Accepted = -1;

                await _context.SaveAsync();

                return true;
            }
            catch (Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "TherapistApplicationsFunctionsProvider", "RejectApplicationAsync");
                return false;
            }
        }
    }
}
