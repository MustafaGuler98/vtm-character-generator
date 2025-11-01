using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{

	public class AttributeCategory : IHasTags
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("tags")]
		public List<string> Tags { get; set; } = new List<string>();


		[JsonPropertyName("attributes")]
		public List<VtMAttribute> Attributes { get; set; } = new List<VtMAttribute>();
	}
}