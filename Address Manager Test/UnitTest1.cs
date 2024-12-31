using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Address_Manager.Window;
using System;


namespace Address_Manager_Test
{
    [TestClass]
    public class CsvParserTests
    {
        [TestMethod]
        public void LoadCsvColumns_ValidCsv_ReturnsColumnHeaders()
        {
            // Arrange
            var csvContent = "Vorname;Name;Strasse\nJohn;Doe;Main Street";
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, csvContent);

            var validateColumns = new ValidateColumns(filePath);

            // Act
            var headers = validateColumns.LoadCsvColumns(filePath);

            // Assert
            Assert.AreEqual(3, headers.Count);
            Assert.IsTrue(headers.Contains("Vorname"));
            Assert.IsTrue(headers.Contains("Name"));
            Assert.IsTrue(headers.Contains("Strasse"));

            // Cleanup
            File.Delete(filePath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertValue_InvalidIntString_ThrowsException()
        {
            // Arrange
            var validateColumns = new ValidateColumns(null);

            // Act
            validateColumns.ConvertValue("Invalid", typeof(int));
        }
    }
    
}