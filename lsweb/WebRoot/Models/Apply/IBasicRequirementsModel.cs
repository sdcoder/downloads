using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightStreamWeb.Models.Apply
{
    public interface IBasicRequirementsModel
    {
        List<FirstAgain.Domain.SharedTypes.ContentManagement.ApplyPage.BasicDynamicRequirement> LoanPurposeBasedBasicRequirements { get; }
    }
}
