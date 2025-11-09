using LibraryAPI.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryAPITests.UnitTests.Validations
{   
    [TestClass]
    public class FirstCapitalLetterAttributeTest
    {
        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void IsValid_ReturnSuccess_IfValueIsEmptyOrNull(string value)
        {
            // Preparation

            var firstCapitalLetterAttribute = new FirstCapitalLetterAttribute();
            var validationContext = new ValidationContext(new object());

            // Test

            var result = firstCapitalLetterAttribute.GetValidationResult(value, validationContext);

            // Validation

            Assert.AreEqual(expected: ValidationResult.Success, actual: result);
        }

        [TestMethod]
        [DataRow("Juan")]
        public void IsValid_ReturnSuccess_IfFirstLetterOfValueIsCapital(string value)
        {
            // Preparation

            var firstCapitalLetterAttribute = new FirstCapitalLetterAttribute();
            var validationContext = new ValidationContext(new object());

            // Test

            var result = firstCapitalLetterAttribute.GetValidationResult(value, validationContext);

            // Validation

            Assert.AreEqual(expected: ValidationResult.Success, actual: result);
        }

        [TestMethod]
        [DataRow("juan")]
        public void IsValid_ReturnError_IfFirstLetterOfValueIsNotCapital(string value)
        {
            // Preparation

            var firstCapitalLetterAttribute = new FirstCapitalLetterAttribute();
            var validationContext = new ValidationContext(new object());

            // Test

            var result = firstCapitalLetterAttribute.GetValidationResult(value, validationContext);

            // Validation

            Assert.AreEqual(expected: "The first letter must be capitalized", actual: result!.ErrorMessage);
        }
    }


}
