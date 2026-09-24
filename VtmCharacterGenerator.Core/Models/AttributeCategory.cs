using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VtmCharacterGenerator.Core.Models
{

	public class AttributeCategory : IHasTags
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = string.Empty;

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("description")]
		public string Description { get; set; } = string.Empty;

		[JsonPropertyName("tags")]
		public List<string> Tags { get; set; } = new();


		[JsonPropertyName("attributes")]
		public List<VtMAttribute> Attributes { get; set; } = new();
	}
}
