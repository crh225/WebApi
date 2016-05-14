using Microsoft.AspNetCore.OData.Builder;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
    public static class ODataConfigurator
    {
        public static void ConfigureODataSample(ODataConventionModelBuilder builder)
        {
            // OData actions are HTTP POST
            // OData functions are HTTP GET

            builder.Namespace = "Sample";
            //builder.AddSerializeInterceptor<string>(interceptor =>
            //{
            //	if (interceptor.DeclaringType == typeof(Order))
            //	{
            //		interceptor.Value = (interceptor.Value ?? "") + "!";
            //	}
            //	return true;
            //});
            builder.EntityType<Order>()
                .Property(p => p.Title)
                //.UseSerializer(value =>
                //{
                //	value.Value = "Hey";
                //	return true;
                //})
                ;
            builder.EntityType<ApplicationUser>()
                .RemoveAllProperties()
                //.AddProperty(p => p.Roles)
                .AddProperty(p => p.UserName)
                .AddProperty(p => p.Email)
                .AddProperty(p => p.FavouriteProductId)
                .AddProperty(p => p.FavouriteProduct)
                ;
            builder
                .EntityType<Order>()
                .HasKey(o => o.Id);
            builder
                .EntityType<Customer>()
                .Property(p => p.CustomerId)
                ;
            builder
                .EntityType<Product>()
                ;
            //builder.EntityType<Product>()
            //	.HasKey(p => p.ProductId);
            builder.EntityType<ApplicationUser>()
                .HasKey(p => p.Id);
            builder
                .Function("HelloWorld")
                .Returns<string>();
            builder
                .Function("HelloComplexWorld")
                .Returns<Permissions>();
            var multiplyFunction = builder
                .Function("Multiply");
            multiplyFunction
                .Parameter<float>("a");
            multiplyFunction
                .Parameter<float>("b");
            multiplyFunction
                .Returns<float>();
            builder
                .EntityType<Product>()
                .Collection
                .Function("MostExpensive")
                .Returns<double>();
            var getProductNameFunction =
                builder
                    .EntityType<Product>()
                    .Function("GetName")
                    .Returns<string>();
            getProductNameFunction
                .Parameter<string>("prefix");
            var postProductNameFunction =
                builder
                    .EntityType<Product>()
                    .Action("PostName")
                    .Returns<string>();
            postProductNameFunction
                .Parameter<string>("prefix");
            builder
                .EntityType<Product>()
                .Collection
                .Function("MostExpensive2")
                .Returns<double>();
            builder
                .EntityType<Product>()
                .Function("ShortName")
                .Returns<string>();
            var validateField =
                builder
                    .Action("ValidateField")
                    .Returns<string>();
            validateField.Parameter<string>("SetName");
            validateField.Parameter<string>("Name");
            validateField.Parameter<string>("Value");
            //builder
            //	.EntityType<Product>()
            //	.Collection
            //	.Action("ValidateField")
            //	;
            //builder
            //	.EntityType<Product>()
            //	.Collection
            //	.Action("ValidateField")
            //	;
            //validateField.Parameter<string>("Name");
            //validateField.Parameter<string>("Value");

            builder
                .EntityType<Order>()
                .Function("DuplicateMethodName")
                .Returns<string>();
            builder
                .EntityType<Product>()
                .Function("DuplicateMethodName")
                .Returns<string>();
        }
    }
}