using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.PWM;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class PWMCollateralPageModel : SunTrustApplyPageModel
    {
        public PWMCollateralPageModel(ICurrentUser user)
            : base(user)
        {
            BodyClass = "apply";
        }

        public List<CollateralAsset> Assets
        {
            get
            {
                var content = new ContentManager().Get<PWMCollateralAssets>();
                if (content != null && content.Assets != null && content.Assets.Any())
                {
                    return content.Assets;
                }

                return new List<CollateralAsset>();
            }

        }

}
}