using System;
using System.Web.Optimization;

namespace LightStreamWeb
{
    public class BundleConfig
    {
        public static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
        {
            if (ignoreList == null)
                throw new ArgumentNullException("ignoreList");
            ignoreList.Ignore("*.intellisense.js");
            ignoreList.Ignore("*-vsdoc.js");
            ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
            //ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
            //ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
        }
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;
            bundles.IgnoreList.Clear();
            AddDefaultIgnorePatterns(bundles.IgnoreList);

            bundles.Add(new StyleBundle("~/bundles/css-third-party").Include(
                            "~/content/foundation.css",
                            "~/content/foundation-6-fixes.css",
                            "~/content/normalize.min.css",
                            "~/content/font-awesome.min.css",
                            "~/content/responsive-tables.css"
                            ));

            bundles.Add(new StyleBundle("~/bundles/css-jquery-ui").Include(
                            "~/scripts/jquery-ui-1.12.1/jquery-ui.min.css"
                            ));

            bundles.Add(new StyleBundle("~/bundles/css-responsive-main-wide").Include(
                            "~/content/style.css",
                            "~/content/counter.css",
                            "~/content/style-lightstream-full-width.css",
                            "~/content/mobile-first.css"
                            ));

            bundles.Add(new StyleBundle("~/bundles/css-responsive-main").Include(
                            "~/content/style.css",
                            "~/content/counter.css",
                            "~/content/style-lightstream.css",
                            "~/content/lightstream.nav.css",
                            "~/content/mobile-first.css"
                            ));

            bundles.Add(new StyleBundle("~/bundles/css-account-services").Include(
                            "~/content/account.css",
                            "~/content/account-lightstream.css"
                            ));
            bundles.Add(new ScriptBundle("~/bundles/scripts-application-status").Include(
                            "~/scripts/angular/angular-route.js",
                            "~/scripts/app/ApplicationStatusModule.js",
                            "~/scripts/app/DeclineReferralOptInController.js",
                            "~/scripts/app/ENoticesController.js",
                            "~/scripts/app/RatesModalController.js",
                            "~/scripts/app/VINController.js",
                            "~/scripts/app/PropertyBeingImprovedController.js",
                            "~/scripts/app/VerificationUploadController.js",
                            "~/scripts/app/LoanAcceptanceController.js",
                            "~/scripts/app/CustomerIdentificationController.js",
                            "~/scripts/jquery-plugins/jquery.signaturepad.js",
                            "~/scripts/jquery-ui-1.12.1/jquery-ui.min.js",
                            "~/scripts/app/global-account-services.js",
                            "~/scripts/app/services/SessionTimeoutService.js"

                            ));
            bundles.Add(new ScriptBundle("~/bundles/scripts-pre-funding").Include(
                            "~/scripts/angular/angular-route.js",
                            "~/scripts/app/ApplicationStatusModule.js",
                            "~/scripts/app/ENoticesController.js",
                            "~/scripts/app/RatesModalController.js",
                            "~/scripts/jquery-plugins/jquery.signaturepad.js",
                            "~/scripts/jquery-ui-1.12.1/jquery-ui.min.js",
                            "~/scripts/app/global-account-services.js",
                            "~/scripts/app/services/SessionTimeoutService.js"
                            ));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/scripts/angular/angular.js",
                "~/scripts/angular/angular-route.js"
                ));

            /* This main-page bundle is repetitive of scripts-app-main bundle but was started when 
               we began to have more than just one form (rate-calculator) on landing pages.  
               Some angular items need to be instantiated before page loads for use on various forms.
             */
            bundles.Add(new ScriptBundle("~/bundles/main-page").Include(
                            "~/scripts/polyfill/es5-shim.min.js",
                            "~/scripts/polyfill/es6-shim.min.js",
                            "~/scripts/app/app.js",
                            "~/scripts/angular/angular-webstorage.js",
                            "~/scripts/app/ls.services.js",
                            "~/scripts/app/ls.filters.js",
                            "~/scripts/app/landing-page.js",
                            "~/scripts/app/subscribe/subscribe-controller.js",
                            "~/scripts/app/subscribe/subscribe-service.js",
                            "~/scripts/app/global.ajaxsettings.js",
                            "~/scripts/app/services/loanAppSessionService.js",
                            "~/scripts/polyfill/url-search-params.js",
                            "~/scripts/polyfill/es6-promise.js",
                            "~/scripts/polyfill/es6-promise.auto.js",
                            "~/scripts/app/fact/FactHistory.js",
                            "~/scripts/app/browser-update.js"
                            ));
            bundles.Add(new ScriptBundle("~/bundles/foundation-global").Include(
                            "~/scripts/foundation/foundation.js",
                            "~/scripts/app/global.js",
                            "~/scripts/app/lazyload.js"
                            ));

            bundles.Add(new ScriptBundle("~/bundles/angular-file-upload").Include(
                            "~/scripts/angular-file-upload/angular-file-upload-shim.js",
                            "~/scripts/angular/angular.js",
                            "~/scripts/angular-file-upload/angular-file-upload.js"
                            ));

            bundles.Add(new ScriptBundle("~/bundles/es5-shim").Include(
                "~/scripts/polyfill/es5-shim.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/es6-shim").Include(
                "~/scripts/polyfill/es6-shim.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/scripts-app-main").Include(
                            "~/scripts/modernizr-custom-2.6.2.js",
                            "~/scripts/polyfill/url-search-params.js",
                            "~/scripts/app/ProperCase.js",
                            "~/scripts/angular/angular-webstorage.js",
                            "~/scripts/app/app.js",
                            "~/scripts/app/ls.services.js",
                            "~/scripts/app/subscribe/subscribe-controller.js",
                            "~/scripts/app/subscribe/subscribe-service.js",
                            "~/scripts/polyfill/es6-promise.js",
                            "~/scripts/polyfill/es6-promise.auto.js",
                            "~/scripts/app/fact/FactHistory.js",
                            "~/scripts/app/global.ajaxsettings.js",
                            "~/scripts/app/services/loanAppSessionService.js",
                            "~/scripts/app/ls.filters.js",
                            "~/scripts/jquery-ui-1.12.1/jquery-ui.min.js",
                            "~/scripts/app/ls.constants.js",
                            "~/scripts/app/ls.directives.js",
                            "~/scripts/app/adaController.js",
                            "~/scripts/app/landing-page.js",
                            "~/scripts/app/browser-update.js"
                          ));

            bundles.Add(new StyleBundle("~/bundles/styles-rate-calculator").Include(
                            "~/Content/rate-calculator.css"));

            bundles.Add(new ScriptBundle("~/bundles/scripts-rate-calculator").Include(
                            "~/scripts/app/rate-calculator.js"));
            bundles.Add(new ScriptBundle("~/bundles/scripts-apply").Include(
                            "~/scripts/app/loan-application.js"));
            bundles.Add(new ScriptBundle("~/bundles/scripts-faq").Include(
                            "~/scripts/jquery-plugins/jquery.highlight.js",
                            "~/scripts/app/faq.js"));

            bundles.Add(new ScriptBundle("~/bundles/scripts-account-services").Include(
                            "~/scripts/angular/angular-route.js",
                            "~/scripts/app/ModalControllers.js",
                            "~/scripts/app/AccountServicesController.js",
                            "~/scripts/app/global-account-services.js",
                            "~/scripts/app/global.ajaxsettings.js",
                            "~/scripts/app/services/SessionTimeoutService.js"
                            ));

            bundles.Add(new ScriptBundle("~/bundles/scripts-account-preferences").Include(
                            "~/scripts/angular/angular-route.js",
                            "~/scripts/app/ui-utils.js",
                            "~/scripts/app/accountPreferences.js",
                            "~/scripts/app/preferences/preferences.js",
                            "~/scripts/app/preferences/contactInformationController.js",
                            "~/scripts/app/preferences/accountLockController.js",
                            "~/scripts/app/preferences/privacyPreferencesController.js",
                            "~/scripts/app/preferences/securityInformationController.js"));

            bundles.Add(new ScriptBundle("~/bundles/scripts-account-services-vin-entry").Include(
                            "~/scripts/app/ApplicationStatusModule.js",
                            "~/scripts/app/AccountServicesController.js",
                            "~/scripts/app/VerificationUploadController.js",
                            "~/scripts/app/VINController.js",
                            "~/scripts/app/global-account-services.js",
                            "~/scripts/app/services/SessionTimeoutService.js",
                            "~/scripts/app/CustomerIdentificationController.js"
                            ));

            bundles.Add(new StyleBundle("~/bundles/css-suntrust-process-replacement").Include(
                            "~/content/suntrust-process-replacement.css"
                            ));

            bundles.Add(new StyleBundle("~/bundles/css-template-redesign").Include(
                            "~/Content/OwlCarousel2-2.3.4/owl.carousel.min.css",
                            "~/Content/OwlCarousel2-2.3.4/owl.theme.default.min.css",
                            "~/Content/redesign/redesign.css"
                            ));

            bundles.Add(new StyleBundle("~/bundles/paidsearch").Include("~/Content/redesign/paidsearch.css"));
            bundles.Add(new StyleBundle("~/bundles/paidsearchbanner-tweaks").Include("~/Content/redesign/paidsearchbanner-tweaks.css"));

            bundles.Add(new ScriptBundle("~/bundles/scripts-template-redesign").Include(
                            "~/Scripts/OwlCarousel2-2.3.4/owl.carousel.min.js",
                            "~/Scripts/redesign/arrive.min.js",
                            "~/Scripts/redesign/redesign.js"
                            ));

            bundles.Add(new ScriptBundle("~/bundles/redesign-scripts").Include(
                "~/Scripts/OwlCarousel2-2.3.4/owl.carousel.min.js",
                "~/Scripts/redesign/arrive.min.js",
                "~/Scripts/polyfill/pep.js",
                "~/Scripts/redesign/redesign.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/auto-landing-scripts").Include(
                "~/Scripts/OwlCarousel2-2.3.4/owl.carousel.min.js",
                "~/Scripts/redesign/arrive.min.js",
                "~/Scripts/redesign/redesign.js",
                "~/Scripts/redesign/autolandingpage.js",
                "~/Scripts/app/RateDataController.js"));

            bundles.Add(new ScriptBundle("~/bundles/ratedatacontroller").Include("~/Scripts/app/RateDataController.js"));

            bundles.Add(new ScriptBundle("~/bundles/stacked-cta-bar").Include("~/scripts/app/stacked-cta-bar.js"));
            bundles.Add(new ScriptBundle("~/bundles/youtube-video").Include("~/scripts/jquery-plugins/jquery.ls.youtube.js"));

            bundles.Add(new StyleBundle("~/bundles/redesign-landing-page-styles").Include(
                            "~/Content/OwlCarousel2-2.3.4/owl.carousel.min.css",
                            "~/Content/OwlCarousel2-2.3.4/owl.theme.default.min.css",
                            "~/Content/redesign/redesign-test.css",
                            "~/Content/redesign/redesignmobile.css",
                            "~/Content/redesign/autolandingpage.css"));

            bundles.Add(new StyleBundle("~/bundles/three-feature-bullets").Include(
                            "~/Content/redesign/three-feature-bullets.css"));

            bundles.Add(new StyleBundle("~/bundles/redesign-page-styles").Include(
                            "~/Content/OwlCarousel2-2.3.4/owl.carousel.min.css",
                            "~/Content/OwlCarousel2-2.3.4/owl.theme.default.min.css",
                            "~/Content/redesign/redesign-test.css",
                            "~/Content/redesign/redesignmobile.css"));


            bundles.Add(new StyleBundle("~/bundles/sunset-ahip").Include("~/Content/redesign/sunsetahip.css"));

            bundles.Add(new ScriptBundle("~/bundles/scripts-blog").Include(
                             "~/scripts/app/blog/blog-service.js",
                             "~/scripts/app/blog/blog-controller.js"
                             ));

            /* 
             * Explicitly enable bundling and minification here, otherwize the un-minified versions
             * will get deployed if the debug version is deployed, and currently Octopus doesn't enforce release version deployments.
             * 
             * Run DEBUG_SINGLE_MACHINE to disable bunding / minification */
#if DEBUG_SINGLE_MACHINE || DEBUG_LOCALHOST
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif


        }
    }
}
