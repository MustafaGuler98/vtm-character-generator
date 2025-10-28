using System.Collections.Generic;

namespace VtmCharacterGenerator.Core.Models
{
    public interface IHasTags
    {
        List<string> Tags { get; set; }
    }
}