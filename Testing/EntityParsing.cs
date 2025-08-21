using ModelBuilder;

namespace Testing;

[TestClass]
public class EntityParsing
{
	[TestMethod]
	public void Species()
	{
		var schema = new ResourceEntityEnumerator([
			"Clinic.md",
			"Species.md",
			"Breed.md",
			"AppSpecies.md"
		]);
			
		var entities = new EntityParser(schema).ParseEntities();
	}
}
