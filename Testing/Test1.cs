using EFBuilder;

namespace Testing;

[TestClass]
public sealed class Test1
{
	[TestMethod]
	public void TestCase01Generation()
	{
		// Arrange
		var service = new EFBuilderService();
		var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
		var inputPath = Path.Combine(projectRoot, "Resources", "Case01", "input.txt");
		var input = File.ReadAllText(inputPath);
		
		// Act
		var generatedFiles = service.GenerateEntitiesFromInput(input, "Testing.Resources.Case01");
		
		// Assert
		Assert.AreEqual(3, generatedFiles.Count, "Should generate 3 entity files");
		Assert.IsTrue(generatedFiles.ContainsKey("Customer.cs"), "Should generate Customer.cs");
		Assert.IsTrue(generatedFiles.ContainsKey("Status.cs"), "Should generate Status.cs");
		Assert.IsTrue(generatedFiles.ContainsKey("Order.cs"), "Should generate Order.cs");
		
		// Verify Customer class content
		var customerCode = generatedFiles["Customer.cs"];
		Assert.IsTrue(customerCode.Contains("public class Customer : BaseTable"), "Customer should inherit from BaseTable");
		Assert.IsTrue(customerCode.Contains("public string FirstName { get; set; } = default!;"), "Customer should have FirstName property");
		Assert.IsTrue(customerCode.Contains("public string? Email { get; set; };"), "Customer should have nullable Email property");
		Assert.IsTrue(customerCode.Contains("public bool IsActive { get; set; } = true;"), "Customer should have IsActive with default value");
		Assert.IsTrue(customerCode.Contains("public ICollection<Order> Orders { get; set; } = [];"), "Customer should have Orders navigation property");
		
		// Verify Status class content
		var statusCode = generatedFiles["Status.cs"];
		Assert.IsTrue(statusCode.Contains("public class Status : BaseTable"), "Status should inherit from BaseTable");
		Assert.IsTrue(statusCode.Contains("public string Name { get; set; } = default!;"), "Status should have Name property");
		Assert.IsTrue(statusCode.Contains("public string? Description { get; set; };"), "Status should have nullable Description property");
		
		// Verify Order class content
		var orderCode = generatedFiles["Order.cs"];
		Assert.IsTrue(orderCode.Contains("public class Order : BaseTable"), "Order should inherit from BaseTable");
		Assert.IsTrue(orderCode.Contains("public int CustomerId { get; set; };"), "Order should have CustomerId foreign key");
		Assert.IsTrue(orderCode.Contains("public Customer? Customer { get; set; }"), "Order should have Customer navigation property");
		Assert.IsTrue(orderCode.Contains("public Status? Status { get; set; }"), "Order should have Status navigation property");
		Assert.IsTrue(orderCode.Contains("public DateTime? StatusDate { get; set; };"), "Order should have nullable StatusDate");
	}
}
