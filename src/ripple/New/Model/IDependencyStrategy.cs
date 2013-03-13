﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FubuCore;

namespace ripple.New.Model
{
	public interface IDependencyStrategy
	{
		bool Matches(Project project);
		IEnumerable<Dependency> Read(Project project);

		void Write(Project project);
	}

	public class RippleDependencyStrategy : IDependencyStrategy
	{
		public const string RippleDependenciesConfig = "ripple.dependencies.config";

		private readonly IFileSystem _fileSystem = new FileSystem();

		public string FileFor(Project project)
		{
			return Path.Combine(project.Directory, RippleDependenciesConfig);
		}

		public bool Matches(Project project)
		{
			return _fileSystem.FileExists(FileFor(project));
		}

		public IEnumerable<Dependency> Read(Project project)
		{
			var dependencies = new List<Dependency>();
			_fileSystem.ReadTextFile(FileFor(project), line =>
			{
				if (line.IsEmpty()) return;

				var values = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				var version = string.Empty;

				if (values.Length == 2)
				{
					version = values[1].Trim();
				}

				dependencies.Add(new Dependency(values[0].Trim(), version));
			});
			
			return dependencies;
		}

		public void Write(Project project)
		{
			var dependencies = new StringBuilder();
			project.Dependencies.Each(dependency => dependencies.AppendLine(dependency.ToString()));

			_fileSystem.WriteStringToFile(FileFor(project), dependencies.ToString());
		}
	}
}