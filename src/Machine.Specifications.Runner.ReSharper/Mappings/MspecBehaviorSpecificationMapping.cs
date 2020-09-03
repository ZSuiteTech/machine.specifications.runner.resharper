﻿using System.Linq;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework.Launch;
using JetBrains.Util;
using Machine.Specifications.Runner.ReSharper.Elements;
using Machine.Specifications.Runner.ReSharper.Tasks;

namespace Machine.Specifications.Runner.ReSharper.Mappings
{
    [SolutionComponent]
    public class MspecBehaviorSpecificationMapping : MspecElementMapping<BehaviorSpecificationElement, MspecBehaviorSpecificationRemoteTask>
    {
        public MspecBehaviorSpecificationMapping(MspecServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected override MspecBehaviorSpecificationRemoteTask ToRemoteTask(BehaviorSpecificationElement element, IUnitTestRun run)
        {
            var task = MspecBehaviorSpecificationRemoteTask.ToClient(
                element.Id.Id,
                element.Children.All(x => run.Launch.Criterion.Criterion.Matches(x)),
                run.Launch.Criterion.Explicit.Contains(element));

            task.ContextTypeName = element.Behavior.Context.TypeName.FullName;
            task.SpecificationFieldName = element.FieldName;
            task.BehaviorFieldName = element.Behavior.FieldName;

            return task;
        }

        protected override BehaviorSpecificationElement ToElement(MspecBehaviorSpecificationRemoteTask task, IUnitTestRun run, IProject project, UnitTestElementFactory factory)
        {
            if (task.ContextTypeName == null)
            {
                run.Launch.Output.Warn("Cannot create element for BehaviorSpecificationElement '" + task.TestId + "': ContextTypeName is missing");

                return null;
            }

            if (task.BehaviorFieldName == null)
            {
                run.Launch.Output.Warn("Cannot create element for BehaviorSpecificationElement '" + task.TestId + "': BehaviorFieldName is missing");

                return null;
            }

            var contextId = ServiceProvider.CreateId(project, run.TargetFrameworkId, task.ContextTypeName);
            var behaviorId = ServiceProvider.CreateId(project, run.TargetFrameworkId, $"{task.ContextTypeName}::{task.BehaviorFieldName}");

            var context = ServiceProvider.ElementManager.GetElementById<ContextElement>(contextId);
            var behavior = ServiceProvider.ElementManager.GetElementById<BehaviorElement>(behaviorId);

            if (context == null)
            {
                run.Launch.Output.Warn("Cannot create element for BehaviorSpecificationElement '" + task.TestId + "': Context is missing");
                return null;
            }

            if (behavior == null)
            {
                run.Launch.Output.Warn("Cannot create element for BehaviorSpecificationElement '" + task.TestId + "': Behavior is missing");
                return null;
            }

            return factory.GetOrCreateBehaviorSpecification(
                project,
                behavior,
                new ClrTypeName(task.ContextTypeName),
                task.SpecificationFieldName,
                false);
        }
    }
}
