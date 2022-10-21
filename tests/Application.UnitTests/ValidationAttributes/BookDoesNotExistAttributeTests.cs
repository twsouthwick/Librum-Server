using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.ActionFilters;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.ValidationAttributes;

public class BookDoesNotExistAttributeTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    
    private readonly Mock<ILogger<BookDoesNotExistAttribute>> _loggerMock = new();
    private readonly BookDoesNotExistAttribute _bookDoesNotExistAttribute;


    public BookDoesNotExistAttributeTests()
    {
        _bookDoesNotExistAttribute = 
            new BookDoesNotExistAttribute(_userRepositoryMock.Object,
                                          _loggerMock.Object);
    }
    
    
        [Fact]
    public async Task ValidateBookDoesNotExist_ShouldSucceed_WhenBookDoesNotExists()
    {
        // Arrange
        var bookInDto = new BookInDto
        {
            Title = "SomeOtherBook"
        };
        
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = "SomeBook" }
            }
        };

        var modelState = new ModelStateDictionary();
        var httpContextMock = new DefaultHttpContext();

        var actionContext = new ActionContext(
            httpContextMock,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            modelState
        );

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            modelState
        );

        executingContext.ActionArguments.Add("SomeDto", bookInDto);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _bookDoesNotExistAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateBookDoesNotExist_ShouldFail_WhenBookExists()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        
        var bookInDto = new BookInDto
        {
            Guid = bookGuid.ToString()
        };
        
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuid }
            }
        };

        var modelState = new ModelStateDictionary();
        var httpContextMock = new DefaultHttpContext();

        var actionContext = new ActionContext(
            httpContextMock,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            modelState
        );

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            modelState
        );

        executingContext.ActionArguments.Add("someDto", bookInDto);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _bookDoesNotExistAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context));

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateBookDoesNotExist_ShouldThrow_WhenNoBookTitleFound()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var httpContextMock = new DefaultHttpContext();

        var actionContext = new ActionContext(
            httpContextMock,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            modelState
        );

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            modelState
        );
        
        
        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => 
            _bookDoesNotExistAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context)));
    }
}