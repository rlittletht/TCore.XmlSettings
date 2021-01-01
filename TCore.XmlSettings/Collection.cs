using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("RefApp")]

namespace TCore.XmlSettings
{
	// A collection of settings files, based on the criteria given
	public class Collection
	{
		public class FileType
		{
			public string Description { get; set; }
			public string Extension { get; set; }

			public FileType(string description, string extension)
			{
				Description = description;
				Extension = extension;
			}
		}

		public class FileDescription
		{
			public string Extension { get; set; }
			public string FullPath { get; set; }
			public string NameWithExtension => Path.GetFileName(FullPath);
			public string Name
			{
				get
				{
					string sLeaf = Path.GetFileName(FullPath);

					if (Extension.Length == 0)
						return sLeaf;
					
					return sLeaf.Substring(0, sLeaf.Length - Extension.Length);
				}
			}

			private const string s_noExtension = "";
			
			public FileDescription(string fullPath)
			{
				FullPath = fullPath;
				Extension = s_noExtension;
				// automagically detect extension
				int ichLastDot = fullPath.LastIndexOf('.');
				
				if (ichLastDot > 0)
				{
					// now, some of our extensions are "doubled" ("*.ds.xml").
					// so see if there's another dot sufficently close (before the
					// last "\\" and within 3 characters

					int ichSlash = fullPath.LastIndexOf('\\');
					if (ichSlash < ichLastDot)
					{
						int ichPenultimateDot = fullPath.LastIndexOf('.', ichLastDot - 1);

						// can't have an empty filename, so "\\.a.foo" must be ".a" and ".foo"
						if (ichPenultimateDot > ichSlash + 1 && ichPenultimateDot + 5 > ichLastDot)
						{
							Extension = fullPath.Substring(ichPenultimateDot);
						}
						else
						{
							Extension = fullPath.Substring(ichLastDot);
						}
					}
				}
			}
		}

		private List<FileType> m_filetypes;
		private List<string> m_searchDirs;

		public List<FileType> FileTypes => m_filetypes;
		public List<string> SearchDirs => m_searchDirs;

		public Collection() { }

		internal static bool IsPathFullyQualified(string path)
		{
			if (path.StartsWith("\\") || path[1] == ':')
				return true;
			
			return false;
		}
		
		public Collection(IEnumerable<FileType> filetypes, IEnumerable<string> searchDirs)
		{
			m_filetypes = filetypes == null ? new List<FileType>() { new FileType("XML Settings", ".xml") } : new List<FileType>(filetypes);
			if (searchDirs == null)
			{
				m_searchDirs = new List<string>();
				m_searchDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Settings"));
			}
			else
			{
				m_searchDirs = new List<string>();
				foreach (string searchDir in searchDirs)
				{
					if (Path.IsPathRooted(searchDir) || IsPathFullyQualified(searchDir))
					{
						m_searchDirs.Add(searchDir);
					}
					else
					{
						// this is a relative path that we need to resolve using %Documents%\Settings as the root
						m_searchDirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Settings", searchDir));
					}
				}
			}
		}

		private List<FileDescription> m_files;

		public void BuildFileList()
		{
			m_files = new List<FileDescription>();
			
			foreach (string searchDir in m_searchDirs)
			{
				if (!Directory.Exists(searchDir))
					continue;
				
				foreach (string file in Directory.EnumerateFiles(searchDir))
				{
					foreach (FileType filetype in FileTypes)
					{
						if (file.ToLower().EndsWith(filetype.Extension.ToLower()))
						{
							m_files.Add(new FileDescription(file));
							break;
						}
					}
				}
			}
		}

		public void EnsureSettingsDirectoriesCreated()
		{
			foreach (string searchDir in m_searchDirs)
			{
				if (!Directory.Exists(searchDir))
					continue;

				Directory.CreateDirectory(searchDir);
			}
		}

		public string GetFullPathName(string settingName)
		{
			if (!settingName.ToLower().EndsWith(FileTypes[0].Extension.ToLower()))
				settingName = $"{settingName}{FileTypes[0].Extension}";

			return Path.Combine(SearchDirs[0], settingName);
		}
		
		public WriteFile<T> CreateSettingsWriteFile<T>(string settingName)
		{
			TextWriter tw = new StreamWriter(GetFullPathName(settingName));

			return WriteFile<T>.CreateSettingsFile(tw);
		}

		public static ReadFile<T> CreateSettingsReadFile<T>(FileDescription fileDescription)
		{
			return ReadFile<T>.CreateSettingsFile(fileDescription.FullPath);
		}
		
		public IEnumerable<FileDescription> SettingsFiles()
		{
			// build up a list of FileDescriptions based on the search path
			if (m_files == null)
				BuildFileList();

			return m_files;
		}
		
		public static Collection CreateCollection(string description, string extension, string searchDir)
		{
			return new Collection(new List<FileType>() {new FileType(description, extension)}, new string[] {searchDir});
		}
	}
}
