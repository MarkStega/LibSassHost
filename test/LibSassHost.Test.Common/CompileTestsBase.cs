﻿using System;
using System.IO;
#if NETCOREAPP1_0 || NET451

using Microsoft.Extensions.PlatformAbstractions;
#endif

using Xunit;

using LibSassHost.Helpers;

namespace LibSassHost.Test.Common
{
	public abstract class CompileTestsBase
	{
		protected readonly string _filesDirectoryPath;

		protected readonly string _fileExtension;

		protected readonly string _subfolderName;

		protected readonly bool _indentedSyntax;


		protected CompileTestsBase(SyntaxType syntaxType)
		{
#if NETCOREAPP1_0
			TestsInitializer.Initialize();

#endif
#if NETCOREAPP1_0 || NET451
			var appEnv = PlatformServices.Default.Application;
			string baseDirectoryPath = Path.Combine(appEnv.ApplicationBasePath,
#if NETCOREAPP1_0
				"../../../"
#else
				"../../../../"
#endif
			);
#elif NET40
			string baseDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../");
#else
#error No implementation for this target
#endif
			_filesDirectoryPath = Path.GetFullPath(Path.Combine(baseDirectoryPath, "../SharedFiles"));

			if (syntaxType == SyntaxType.Sass)
			{
				_fileExtension = ".sass";
				_subfolderName = "sass";
				_indentedSyntax = true;
			}
			else if (syntaxType == SyntaxType.Scss)
			{
				_fileExtension = ".scss";
				_subfolderName = "scss";
				_indentedSyntax = false;
			}
			else
			{
				throw new NotSupportedException();
			}
		}


		protected static string GetCanonicalPath(string path)
		{
			return PathHelpers.ProcessBackSlashes(Path.GetFullPath(path));
		}

		[Fact]
		public virtual void CodeCompilationIsCorrect()
		{
			// Arrange
			string inputFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("variables/{0}/style{1}", _subfolderName, _fileExtension));
			string outputFilePath = Path.Combine(_filesDirectoryPath, "variables/style.css");

			string inputCode = File.ReadAllText(inputFilePath);
			string targetOutputCode = File.ReadAllText(outputFilePath);

			var options = new CompilationOptions
			{
				IndentedSyntax = _indentedSyntax
			};

			// Act
			CompilationResult result = SassCompiler.Compile(inputCode, options: options);

			// Assert
			Assert.Equal(targetOutputCode, result.CompiledContent);
			Assert.True(string.IsNullOrEmpty(result.SourceMap));
			Assert.Empty(result.IncludedFilePaths);
		}

		[Fact]
		public virtual void CodeWithImportCompilationIsCorrect()
		{
			// Arrange
			string inputFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("import/{0}/base{1}", _subfolderName, _fileExtension));
			string importedFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("import/{0}/_reset{1}", _subfolderName, _fileExtension));
			string outputFilePath = Path.Combine(_filesDirectoryPath, "import/base.css");

			string inputCode = File.ReadAllText(inputFilePath);
			string targetOutputCode = File.ReadAllText(outputFilePath);

			var options = new CompilationOptions
			{
				IndentedSyntax = _indentedSyntax,
				SourceMap = true
			};

			// Act
			CompilationResult result = SassCompiler.Compile(inputCode, inputFilePath, options: options);

			// Assert
			Assert.Equal(targetOutputCode, result.CompiledContent);
			Assert.False(string.IsNullOrEmpty(result.SourceMap));

			var includedFilePaths = result.IncludedFilePaths;
			Assert.Equal(1, includedFilePaths.Count);
			Assert.Equal(GetCanonicalPath(importedFilePath), includedFilePaths[0]);
		}

		[Fact]
		public virtual void CodeWithUtf8CharactersCompilationIsCorrect()
		{
			// Arrange
			string inputFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("ютф-8/{0}/символы{1}", _subfolderName, _fileExtension));
			string outputFilePath = Path.Combine(_filesDirectoryPath, "ютф-8/символы.css");

			string inputCode = File.ReadAllText(inputFilePath);
			string targetOutputCode = File.ReadAllText(outputFilePath);

			var options = new CompilationOptions
			{
				IndentedSyntax = _indentedSyntax
			};

			// Act
			CompilationResult result = SassCompiler.Compile(inputCode, options: options);

			// Assert
			Assert.Equal(targetOutputCode, result.CompiledContent);
			Assert.True(string.IsNullOrEmpty(result.SourceMap));
			Assert.Empty(result.IncludedFilePaths);
		}

		[Fact]
		public virtual void FileCompilationIsCorrect()
		{
			// Arrange
			string inputFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("variables/{0}/style{1}", _subfolderName, _fileExtension));
			string outputFilePath = Path.Combine(_filesDirectoryPath, "variables/style.css");

			string targetOutputCode = File.ReadAllText(outputFilePath);

			// Act
			CompilationResult result = SassCompiler.CompileFile(inputFilePath);

			// Assert
			Assert.Equal(targetOutputCode, result.CompiledContent);
			Assert.True(string.IsNullOrEmpty(result.SourceMap));

			var includedFilePaths = result.IncludedFilePaths;
			Assert.Equal(1, includedFilePaths.Count);
			Assert.Equal(GetCanonicalPath(inputFilePath), includedFilePaths[0]);
		}

		[Fact]
		public virtual void FileWithImportCompilationIsCorrect()
		{
			// Arrange
			string inputFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("import/{0}/base{1}", _subfolderName, _fileExtension));
			string importedFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("import/{0}/_reset{1}", _subfolderName, _fileExtension));
			string outputFilePath = Path.Combine(_filesDirectoryPath, "import/base.css");

			string targetOutputCode = File.ReadAllText(outputFilePath);

			var options = new CompilationOptions
			{
				SourceMap = true
			};

			// Act
			CompilationResult result = SassCompiler.CompileFile(inputFilePath, options: options);

			// Assert
			Assert.Equal(targetOutputCode, result.CompiledContent);
			Assert.False(string.IsNullOrEmpty(result.SourceMap));

			var includedFilePaths = result.IncludedFilePaths;
			Assert.Equal(2, includedFilePaths.Count);
			Assert.Equal(GetCanonicalPath(inputFilePath), includedFilePaths[0]);
			Assert.Equal(GetCanonicalPath(importedFilePath), includedFilePaths[1]);
		}

		[Fact]
		public virtual void FileWithUtf8CharactersCompilationIsCorrect()
		{
			// Arrange
			string inputFilePath = Path.Combine(_filesDirectoryPath,
				string.Format("ютф-8/{0}/символы{1}", _subfolderName, _fileExtension));
			string outputFilePath = Path.Combine(_filesDirectoryPath, "ютф-8/символы.css");

			string targetOutputCode = File.ReadAllText(outputFilePath);

			// Act
			CompilationResult result = SassCompiler.CompileFile(inputFilePath);

			// Assert
			Assert.Equal(targetOutputCode, result.CompiledContent);
			Assert.True(string.IsNullOrEmpty(result.SourceMap));

			var includedFilePaths = result.IncludedFilePaths;
			Assert.Equal(1, includedFilePaths.Count);
			Assert.Equal(GetCanonicalPath(inputFilePath), includedFilePaths[0]);
		}
	}
}