using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable;
using MockQueryable.Moq;
using PDBT.Controllers;
using PDBT.Data;
using PDBT.Models;
using PDBT.Repository;
using Xunit;


namespace PDBT.Tests;

public class IssueControllerTests
{
    private readonly IssueController _sut;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new Mock<IUnitOfWork>(); 
    private readonly IEnumerable<Issue> SampleIssueData = new List<Issue>()
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
        },
        new Issue()
        {
            Id = 2,
            IssueName = "Example 2",
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
            Priority = IssuePriority.Low,
            TimeForCompletion = DateTime.Now,
            Type = IssueType.NewFeature
        }
    }.AsEnumerable();

    private IssueDTO sampleDto = new IssueDTO()
    {
        Id = 1,
        IssueName = "Example",
        Description = "This is an example",
        DueDate = DateTime.Now,
        Labels = new List<LabelDTO>()
        {
            new LabelDTO()
            {
                Id = 1
            }
        },
        Priority = IssuePriority.Medium,
        TimeForCompletion = DateTime.Now,
        Type = IssueType.Bug
    };
    
    public IssueControllerTests()
    {
        _sut = new IssueController(_unitOfWorkMock.Object);
    }
    
    [Fact]
    public async Task GetIssues_ShouldReturnIssues_WhenIssuesExist()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.Issues.GetAllAsync().Result).Returns(SampleIssueData);
        
        // Act 
        var issues = await _sut.GetIssues();

        // Assert
        Assert.Equal(SampleIssueData.First(), issues.Value.First());
    }

    [Fact]
    public async Task GetIssues_ShouldReturn404_WhenNoIssuesExists()
    {
        // Arrange
        var notfound = new NotFoundResult();
        var emptyList = new List<Issue>() {};
        _unitOfWorkMock.Setup(uow => uow.Issues.GetAllAsync().Result).Returns(emptyList);

        // Act
        var issues = await _sut.GetIssues();

        // Assert
        Assert.IsType<NotFoundResult>(issues.Result);
    }

    [Fact]
    public async Task GetIssue_ShouldReturnIssue_WhenIssueExist()
    {
        // Arrange
        int id = 2;
        var result = SampleIssueData.First(i => i.Id == id);
        _unitOfWorkMock.Setup(uow => uow.Issues.GetByIdAsync(id).Result).Returns(result);
        
        // Act 
        var issues = await _sut.GetIssue(id);

        // Assert
        Assert.Equal(result, issues.Value);
    }

    [Fact]
    public async Task GetIssue_ShouldReturn404_WhenNoIssueExist()
    {
        // Arrange
        int id = -99;
        Issue result = null;
        _unitOfWorkMock.Setup(uow => uow.Issues.GetByIdAsync(id).Result).Returns(result);
        
        // Act 
        var issues = await _sut.GetIssue(id);

        // Assert
        Assert.IsType<NotFoundResult>(issues.Result);
    }

    [Fact]
    public async Task PutIssue_ShouldReturn204_WhenDtoAndIdIsVaildWithLabels()
    {
        // Arrange
        var id = 1;
        
        _unitOfWorkMock.Verify(uow => uow.Issues.Update(It.IsAny<Issue>()), Times.AtMost(1));
        _unitOfWorkMock.Setup(uow => uow.Issues.GetByIdAsync(id).Result).Returns(SampleIssueData.First());
        _unitOfWorkMock.Setup(uow => uow.Labels.GetByIdAsync(id).Result).Returns(It.IsAny<Label>());
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync().Result).Returns(1);

        
        // Act
        var issue = await _sut.PutIssue(id, sampleDto);

        // Assert
        Assert.IsType<NoContentResult>(issue);
    }

    [Fact]
    public async Task PutIssue_ShouldReturn400_WhenIdAndDtoMismatch()
    {
        // Arrange
        const int id = 40;

        // Act
        var issue = await _sut.PutIssue(id, sampleDto);

        // Assert
        Assert.IsType<BadRequestResult>(issue);
    }

    [Fact]
    public async Task PutIssue_ShouldReturn204_WhenDtoAndIdIsVaildWithoutLabels()
    {
        // Arrange
        var id = 1;
        var tempDto = sampleDto;
        tempDto.Labels = null;
        
        _unitOfWorkMock.Verify(uow => uow.Issues.Update(It.IsAny<Issue>()), Times.AtMost(1));
        _unitOfWorkMock.Setup(uow => uow.Issues.GetByIdAsync(id).Result).Returns(It.IsAny<Issue>());
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync().Result).Returns(1);

        
        // Act
        var issue = await _sut.PutIssue(id, tempDto);

        // Assert
        Assert.IsType<NoContentResult>(issue);
    }

    [Fact]
    public async Task PutIssue_ShouldReturn404_WhenDbUpdateConcurrencyExceptionIsThrown()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        var emptyList = new List<Issue>() {};
        
        _unitOfWorkMock.Verify(uow => uow.Issues.Update(It.IsAny<Issue>()), Times.AtMost(1));
        _unitOfWorkMock.Setup(uow => uow.Issues.GetByIdAsync(1).Result).Returns(It.IsAny<Issue>());
        _unitOfWorkMock.Setup(uow => uow.Labels.GetByIdAsync(1).Result).Returns(It.IsAny<Label>());
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).Throws<DbUpdateConcurrencyException>();
        _unitOfWorkMock.Setup(uow => uow.Issues.GetAll()).Returns(emptyList);
        
        
        // Act
        var results = await _sut.PutIssue(1, sampleDto);

        // Assert
        Assert.IsType<NotFoundResult>(results);
    }
    
    [Fact]
    public async Task PutIssue_ShouldThorwError_WhenDbUpdateConcurrencyExceptionIsThrown()
    {
        // Arrange
        _unitOfWorkMock.Verify(uow => uow.Issues.Update(It.IsAny<Issue>()), Times.AtMost(1));
        _unitOfWorkMock.Setup(uow => uow.Issues.GetByIdAsync(1).Result).Returns(It.IsAny<Issue>());
        _unitOfWorkMock.Setup(uow => uow.Labels.GetByIdAsync(1).Result).Returns(It.IsAny<Label>());
        _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).Throws<DbUpdateConcurrencyException>();
        _unitOfWorkMock.Setup(uow => uow.Issues.GetAll()).Returns(SampleIssueData);
        
        
        // Act
        Task Act() => _sut.PutIssue(1, sampleDto);

        // Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(Act);
    }
}