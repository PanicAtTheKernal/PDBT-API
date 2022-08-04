using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable;
using MockQueryable.Moq;
using PDBT.Controllers;
using PDBT.Data;
using PDBT.Models;
using Xunit;


namespace PDBT.Tests;

public class IssueControllerTests
{
    private readonly IssueController _sut;
    private readonly Mock<IPdbtContext> _pdbtContextMock = new Mock<IPdbtContext>(); 

    public IssueControllerTests()
    {
        _sut = new IssueController(_pdbtContextMock.Object);
    }
    
    [Fact]
    public async Task GetIssues_ShouldReturnIssues_WhenIssuesExist()
    {
        // Arrange
        var data = new List<Issue>()
        {
            new Issue()
            {
                Id = 1,
                IssueName = "Example",
                Description = "This is an example",
                DueDate = DateTime.Now,
                Labels = new List<Label>()
                {
                    new Label()
                    {
                        Id = 1,
                        Name = "Test"
                    }
                },
                LinkedIssues = null,
                Priority = IssuePriority.Medium,
                TimeForCompletion = DateTime.Now,
                Type = IssueType.Bug
            }
        }.AsQueryable();

        var dbmock = data.AsQueryable().BuildMockDbSet();
        _pdbtContextMock.SetupProperty(context => context.Issues, dbmock.Object);
        
        // Act 
        var issues = await _sut.GetIssues();

        // Assert
        Assert.Equal(issues.Value.First(), data.First());
    }
}