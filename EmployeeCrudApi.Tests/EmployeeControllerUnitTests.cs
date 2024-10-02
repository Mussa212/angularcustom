using EmployeeCrudApi.Controllers;
using EmployeeCrudApi.Data;
using EmployeeCrudApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudApi.Tests
{
    public class EmployeeControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Crear una nueva base de datos en memoria para cada prueba
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfEmployees()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Employees.AddRange(
                new Employee { Id = 1, Name = "John Doe" },
                new Employee { Id = 2, Name = "Jane Doe" }
            );
            context.SaveChanges();

            var controller = new EmployeeController(context);

            // Act
            var result = await controller.GetAll();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("John Doe", result[0].Name);
            Assert.Equal("Jane Doe", result[1].Name);
        }

        [Fact]
        public async Task GetById_ReturnsEmployeeById()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Employees.Add(new Employee { Id = 1, Name = "John Doe" });
            context.SaveChanges();

            var controller = new EmployeeController(context);

            // Act
            var result = await controller.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John Doe", result.Name);
        }

        [Fact]
        public async Task Create_AddsEmployee()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EmployeeController(context);

            var newEmployee = new Employee { Id = 3, Name = "New Employee" };

            // Act
            await controller.Create(newEmployee);

            // Assert
            var employee = await context.Employees.FindAsync(3);
            Assert.NotNull(employee);
            Assert.Equal("New EMPLOYEE", employee.Name); // Formateo esperado
        }

        [Fact]
        public async Task Create_Fails_WhenNameTooShort()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EmployeeController(context);

            var newEmployee = new Employee { Id = 3, Name = "A" };

            // Act
            var result = await controller.Create(newEmployee);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_Fails_WhenNameContainsNumbers()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EmployeeController(context);

            var newEmployee = new Employee { Id = 3, Name = "John123" };

            // Act
            var result = await controller.Create(newEmployee);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_Fails_WhenNameTooLong()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EmployeeController(context);

            var newEmployee = new Employee { Id = 3, Name = new string('a', 101) }; // Nombre con más de 100 caracteres

            // Act
            var result = await controller.Create(newEmployee);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_Fails_WhenNameIsDuplicated()
        {
                // Arrange
                var context = GetInMemoryDbContext();
                var controller = new EmployeeController(context);

                var employee1 = new Employee { Id = 1, Name = "John Doe" };
                var employee2 = new Employee { Id = 2, Name = "John Doe" }; // Nombre duplicado

                context.Employees.Add(employee1);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.Create(employee2);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result); // Controlar duplicados correctamente
        }

        [Fact]
        public async Task Create_FormatsNameCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new EmployeeController(context);

            var newEmployee = new Employee { Id = 3, Name = "juan carlos chamizo" };

            // Act
            await controller.Create(newEmployee);

            // Assert
            var employee = await context.Employees.FindAsync(3);
            Assert.NotNull(employee);
            Assert.Equal("Juan Carlos CHAMIZO", employee.Name);
        }

        [Fact]
        public async Task Update_UpdatesEmployee()
        {
                // Arrange
                var context = GetInMemoryDbContext();
                var existingEmployee = new Employee { Id = 1, Name = "Old Name" };
                context.Employees.Add(existingEmployee);
                context.SaveChanges();

                var controller = new EmployeeController(context);

                var updatedEmployee = new Employee { Id = 1, Name = "Updated Name" };

                // Act
                await controller.Update(updatedEmployee);

                // Assert
                var employee = await context.Employees.FindAsync(1);
                Assert.NotNull(employee);
                Assert.Equal("Updated NAME", employee.Name); // Formateo esperado
        }

        [Fact]
        public async Task Delete_RemovesEmployee()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var employeeToDelete = new Employee { Id = 1, Name = "John Doe" };
            context.Employees.Add(employeeToDelete);
            context.SaveChanges();

            var controller = new EmployeeController(context);

            // Act
            await controller.Delete(1);

            // Assert
            var employee = await context.Employees.FindAsync(1);
            Assert.Null(employee); // Verifica que el empleado fue eliminado
        }
    }
}
