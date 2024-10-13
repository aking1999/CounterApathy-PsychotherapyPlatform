using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Framework.Helpers.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using WebApplication9.Implementations;
using WebApplication9.Interfaces;
using Winton.AspNetCore.Seo.Sitemaps;

namespace WebApplication9.Helpers
{
    public class SeoHelper
    {
        private static IUrlHelper _url => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
        private static ITherapistFunctionsProvider _therapistFunctions => new TherapistFunctionsProvider();

        public static List<SitemapUrlOptions> SitemapUrls
        {
            get
            {
                var sitemapUrls = new List<SitemapUrlOptions>
                {
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("Index", "Home"),
                        Priority = 1M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.AllPsychotherapistsUrl(),
                        Priority = 1M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("PsychotherapistsExperiences", "Therapists"),
                        Priority = 1M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("Index", "Consultations"),
                        Priority = 1M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("BookingRoadmap", "Guides"),
                        Priority = 0.9M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("Register", "Authorization"),
                        Priority = 0.8M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("SignIn", "Authorization"),
                        Priority = 0.8M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("ForgotPassword", "Authorization"),
                        Priority = 0.8M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("FAQ", "Home"),
                        Priority = 0.8M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("About", "Home"),
                        Priority = 0.8M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("CustomerSupport", "Home"),
                        Priority = 0.8M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("All", "Guides"),
                        Priority = 0.8M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("TermsOfService", "Home"),
                        Priority = 0.7M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("TherapistsTerms", "Home"),
                        Priority = 0.7M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("Privacy", "Home"),
                        Priority = 0.7M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("All", "Guides"),
                        Priority = 0.6M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("JoinGoogleMeetByComputer", "Guides"),
                        Priority = 0.6M
                    },
                    new SitemapUrlOptions
                    {
                        RelativeUrl = _url.Action("JoinGoogleMeetByPhone", "Guides"),
                        Priority = 0.6M
                    }
                };

                _therapistFunctions.GetTherapistsWithSetUpAccount()
                                   .Select(t => new { t.Id })
                                   .ToList()
                                   .ForEach(thrId => 
                                                sitemapUrls.Add
                                                (
                                                    new SitemapUrlOptions 
                                                    { 
                                                        RelativeUrl = _url.PsychotherapistPublicProfileUrl(thrId.Id),
                                                        Priority = 0.9M
                                                    }
                                                )
                                           );

                return sitemapUrls;
            }
        }

        public static List<string> DisabledUrls { get; } = new List<string>
        {
            //_url.AllPsychotherapistsUrl("najzakazivaniji", "*"),
            //_url.AllPsychotherapistsUrl("psihoterapijska-tehnika", "*"),
            //_url.AllPsychotherapistsUrl("specijalnost", "*"),
            //_url.AllPsychotherapistsUrl("ocena", "*"),
            //_url.AllPsychotherapistsUrl("cena", "*"),
            "/licencirani-psihoterapeuti/najzakazivaniji/",
            "/licencirani-psihoterapeuti/psihoterapijska-tehnika/",
            "/licencirani-psihoterapeuti/specijalnost/",
            "/licencirani-psihoterapeuti/ocena/",
            "/licencirani-psihoterapeuti/cena/",
            _url.Action("GetConsultations", "Consultations"),
            _url.Action("BookConsultation", "Consultations"),
            _url.Action("ConfirmEmail", "Authorization"),
            _url.Action("EmailConfirmation", "Authorization"),
            _url.Action("PasswordReset", "Authorization"),
            //_url.Action("EasySiteNavigation", "Home"),
            //"/nalog",
            //"/nalog/",
            "/error",
            "/error/",
            "/anonimno",
            "/anonimno/",
            //"/zakazane-seanse",
            //"/zakazane-seanse/",
            //"/notifikacije",
            //"/notifikacije/",
            //"/paypal/",
            //"/terapeut/",
            //"/admin/",
            "/base/",
            //ajax urls
            _url.Action("LogFrontEndErrors", "Error"),
            _url.Action("BookSession", "Therapists", new { Area = "" })
        };

        public static List<string> DisabledContent { get; } = new List<string>
        {
            _url.Content("~/images/content-images/flags/"),
            _url.Content("~/images/content-images/onboarding/"),
            _url.Content("~/images/content-images/medal-star-ribbon.svg"),
            _url.Content("~/images/content-images/shield.svg"),
            _url.Content("~/images/content-images/certificate.svg"),
            _url.Content("~/custom-site-layout/images/envelope.png")
        };
    }
}
