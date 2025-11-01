using System;
using System.Collections.Generic;
using System.Linq;
using VtmCharacterGenerator.Core.Data;
using VtmCharacterGenerator.Core.Models;

namespace VtmCharacterGenerator.Core.Services
{
    public class AttributeService
    {
        private readonly GameDataProvider _dataProvider;
        private readonly AffinityProcessorService _affinityProcessor;

        public AttributeService(GameDataProvider dataProvider, AffinityProcessorService affinityProcessor)
        {
            _dataProvider = dataProvider;
            _affinityProcessor = affinityProcessor;
        }

        public Dictionary<string, int> DistributeAttributes(Dictionary<string, int> affinityProfile)
        {
            // Initialize all attributes with a base score of 1.
            var attributes = _dataProvider.AttributeCategories
                .SelectMany(category => category.Attributes)
                .ToDictionary(attr => attr.Id, attr => 1);

            var pointsToDistribute = new List<int> { 7, 5, 3 };
            var assignedCategoryPoints = new Dictionary<string, int>();
            var remainingCategories = new List<AttributeCategory>(_dataProvider.AttributeCategories);


            // Distribute 7, 5, and 3 points among the three categories using the affinity profile.
            foreach (var points in pointsToDistribute)
            {
                var chosenCategory = _affinityProcessor.GetWeightedRandom(remainingCategories, affinityProfile);
                assignedCategoryPoints[chosenCategory.Name] = points;
                remainingCategories.Remove(chosenCategory);
            }

            // Distribute the assigned points to the individual attributes within each category.
            foreach (var categoryAssignment in assignedCategoryPoints)
            {
                string categoryName = categoryAssignment.Key;
                int points = categoryAssignment.Value;

                var category = _dataProvider.AttributeCategories.FirstOrDefault(c => c.Name == categoryName);
                if (category == null || !category.Attributes.Any()) continue;


                // Distribute points one by one to apply the weighting for each point.
                for (int i = 0; i < points; i++)
                {
                    var availableAttributes = category.Attributes
                        .Where(attr => attributes[attr.Id] < 5)
                        .ToList();

                    // If all attributes in the category are maxed out, we can't distribute more points.
                    if (!availableAttributes.Any())
                    {
                        break;
                    }

                    var chosenAttribute = _affinityProcessor.GetWeightedRandom(availableAttributes, affinityProfile);
                    if (chosenAttribute != null)
                    {
                        attributes[chosenAttribute.Id]++;
                    }
                }
            }

            return attributes;
        }
    }
}