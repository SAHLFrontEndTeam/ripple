﻿using FubuTestingSupport;
using NUnit.Framework;
using ripple.Model;
using ripple.Nuget;

namespace ripple.Testing.Nuget.Installations
{
    [TestFixture]
    public class installing_a_new_fixed_solution_dependency : NugetPlanContext
    {
        private Solution theSolution;
        private NugetPlan thePlan;
        private NugetPlanBuilder theBuilder;

        [SetUp]
        public void SetUp()
        {
            FeedScenario.Create(scenario => scenario.For(Feed.NuGetV2).Add("fubu", "1.2.0.0"));

            theSolution = new Solution();

            theBuilder = new NugetPlanBuilder();

            var request = new NugetPlanRequest
            {
                Solution = theSolution,
                Dependency = new Dependency("fubu", "1.2.0.0"),
                Operation = OperationType.Install,
                Mode = UpdateMode.Fixed
            };

            thePlan = theBuilder.PlanFor(request);
        }

        [TearDown]
        public void TearDown()
        {
            FeedRegistry.Reset();
        }

        [Test]
        public void installs_as_fixed_to_solution()
        {
            thePlan.ShouldHaveTheSameElementsAs(
                solutionInstallation("fubu", "1.2.0.0", UpdateMode.Fixed)
            );
        }
    }
}