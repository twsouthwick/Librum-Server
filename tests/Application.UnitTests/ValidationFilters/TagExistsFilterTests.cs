using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.ActionFilters;
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

namespace Application.UnitTests.ValidationFilters;

public class TagExistsFilterTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<TagExistsFilter>> _loggerMock = new();
    private readonly TagExistsFilter _tagExistsFilterFilter;


    public TagExistsFilterTests()
    {
        _tagExistsFilterFilter = new TagExistsFilter(_userRepositoryMock.Object, 
                                                           _loggerMock.Object);
    }


    [Fact]
    public async Task ATagExistsAttribute_Succeeds()
    {
        // Arrange
        var tagGuid = Guid.NewGuid();
        var user = new User
        {
            Tags = new List<Tag>
            {
                new Tag
                {
                    TagId = tagGuid,
                    Name = "SomeTag"
                }
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

        executingContext.ActionArguments.Add("guid", tagGuid.ToString());

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _tagExistsFilterFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ATagExistsAttribute_FailsIfTagDoesNotExist()
    {
        // Arrange
        var user = new User
        {
            Tags = new List<Tag>
            {
                new Tag
                {
                    TagId = Guid.NewGuid(),
                    Name = "SomeTag"
                }
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

        executingContext.ActionArguments.Add("guid", Guid.NewGuid().ToString());

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _tagExistsFilterFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ATagExistsAttribute_FailsIfNoTagGuidParameterExists()
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
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User());


        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => 
            _tagExistsFilterFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context)));
    }
}