using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisputeReconsile.Infra.Repos;
using DisputeReconsile.Models;
using FluentAssertions;

namespace DisputeReconsile.Tests.Repos
{
    public class InMemDisputeRepoTests
    {
        private readonly InMemDisputeRepo _repo;

        public InMemDisputeRepoTests()
        {
            _repo = new InMemDisputeRepo();
        }

        [Fact]
        public async Task GetAllDisputesAsync_ShouldReturnAllDisputes()
        {
            // Act
            var disputes = await _repo.GetAllDisputesAsync();

            // Assert
            disputes.Should().NotBeEmpty();
            disputes.Should().HaveCount(3); 
        }

        [Fact]
        public async Task GetDisputeByIdAsync_WithExistingId_ShouldReturnDispute()
        {
            // Act
            var dispute = await _repo.GetDisputeByIdAsync("case_001");

            // Assert
            dispute.Should().NotBeNull();
            dispute!.DisputeId.Should().Be("case_001");
            dispute.Amount.Should().Be(100.00m);
        }

        [Fact]
        public async Task GetDisputeByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Act
            var dispute = await _repo.GetDisputeByIdAsync("non_existing");

            // Assert
            dispute.Should().BeNull();
        }

        [Fact]
        public async Task GetDisputesByStatusAsync_WithValidStatus_ShouldReturnFilteredDisputes()
        {
            // Act
            var disputes = await _repo.GetDisputesByStatusAsync("Open");

            // Assert
            var disputeList = disputes.ToList();
            disputeList.Should().NotBeEmpty();
            disputeList.Should().OnlyContain(d => d.Status == "Open");
        }

        [Fact]
        public async Task AddDisputeAsync_ShouldAddDisputeToRepository()
        {
            // Arrange
            var newDispute = new Dispute
            {
                DisputeId = "case_new",
                TransactionId = "txn_new",
                Amount = 200.00m,
                Currency = "USD",
                Status = "Open",
                Reason = "Test"
            };

            // Act
            await _repo.AddDisputeAsync(newDispute);
            var retrievedDispute = await _repo.GetDisputeByIdAsync("case_new");

            // Assert
            retrievedDispute.Should().NotBeNull();
            retrievedDispute!.DisputeId.Should().Be("case_new");
            retrievedDispute.Amount.Should().Be(200.00m);
        }

        [Fact]
        public async Task UpdateDisputeAsync_WithExistingDispute_ShouldUpdateDispute()
        {
            // Arrange
            var updatedDispute = new Dispute
            {
                DisputeId = "case_001",
                TransactionId = "txn_001",
                Amount = 999.00m, // Changed amount
                Currency = "USD",
                Status = "Closed", // Changed status
                Reason = "Fraud"
            };

            // Act
            await _repo.UpdateDisputeAsync(updatedDispute);
            var retrievedDispute = await _repo.GetDisputeByIdAsync("case_001");

            // Assert
            retrievedDispute.Should().NotBeNull();
            retrievedDispute!.Amount.Should().Be(999.00m);
            retrievedDispute.Status.Should().Be("Closed");
        }
    }
}
