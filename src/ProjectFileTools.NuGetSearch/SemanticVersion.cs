﻿using System;

namespace ProjectFileTools.NuGetSearch
{

    /// <summary>
    /// Semantic version 2.0.0 parser per http://semver.org/spec/v2.0.0.html
    /// </summary>
    public class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private readonly int _hashCode;

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public string BuildMetadata { get; private set; }

        public string PrereleaseVersion { get; private set; }

        public string OriginalText { get; private set; }

        private SemanticVersion(string originalText)
        {
            _hashCode = originalText?.GetHashCode() ?? 0;
            OriginalText = originalText;
        }

        public static SemanticVersion Parse(string value)
        {
            SemanticVersion ver = new SemanticVersion(value);

            if (value == null)
            {
                return ver;
            }

            int prereleaseStart = value.IndexOf('-');
            int buildMetadataStart = value.IndexOf('+');

            //If the index of the build metadata marker (+) is greater than the index of the prerelease marker (-)
            //  then it is necessarily found in the string because if both were not found they'd be equal
            if (buildMetadataStart > prereleaseStart)
            {
                //If the build metadata marker is not the last character in the string, take off everything after it
                //  and use it for the build metadata field
                if (buildMetadataStart < value.Length - 1)
                {
                    ver.BuildMetadata = value.Substring(buildMetadataStart + 1);
                }

                value = value.Substring(0, buildMetadataStart);

                //If the prerelease section is found, extract it
                if (prereleaseStart > -1)
                {
                    //If the prerelease section marker is not the last character in the string, take off everything after it
                    //  and use it for the prerelease field
                    if (prereleaseStart < value.Length - 1)
                    {
                        ver.PrereleaseVersion = value.Substring(prereleaseStart + 1);
                    }

                    value = value.Substring(0, prereleaseStart);
                }
            }
            //If the build metadata wasn't the last metadata section found, check to see if a prerelease section exists.
            //  If it doesn't, then neither section exists
            else if (prereleaseStart > -1)
            {
                //If the prerelease version marker is not the last character in the string, take off everything after it
                //  and use it for the prerelease version field
                if (prereleaseStart < value.Length - 1)
                {
                    ver.PrereleaseVersion = value.Substring(prereleaseStart + 1);
                }

                value = value.Substring(0, prereleaseStart);

                //If the build metadata section is found, extract it
                if (buildMetadataStart > -1)
                {
                    //If the build metadata marker is not the last character in the string, take off everything after it
                    //  and use it for the build metadata field
                    if (buildMetadataStart < value.Length - 1)
                    {
                        ver.BuildMetadata = value.Substring(buildMetadataStart + 1);
                    }

                    value = value.Substring(0, buildMetadataStart);
                }
            }

            string[] versionParts = value.Split('.');

            if (versionParts.Length > 0)
            {
                int major;
                int.TryParse(versionParts[0], out major);
                ver.Major = major;
            }

            if (versionParts.Length > 1)
            {
                int minor;
                int.TryParse(versionParts[1], out minor);
                ver.Minor = minor;
            }

            if (versionParts.Length > 2)
            {
                int patch;
                int.TryParse(versionParts[2], out patch);
                ver.Patch = patch;
            }

            return ver;
        }

        public int CompareTo(SemanticVersion other)
        {
            if (other == null)
            {
                return 1;
            }

            int result = Major.CompareTo(other.Major);

            if (result != 0)
            {
                return result;
            }

            result = Minor.CompareTo(other.Minor);

            if (result != 0)
            {
                return result;
            }

            result = Patch.CompareTo(other.Patch);

            if (result != 0)
            {
                return result;
            }

            //A version not marked with prerelease is later than one with a prerelease designation
            if (PrereleaseVersion == null && other.PrereleaseVersion != null)
            {
                return 1;
            }

            //A version not marked with prerelease is later than one with a prerelease designation
            if (PrereleaseVersion != null && other.PrereleaseVersion == null)
            {
                return -1;
            }

            result = StringComparer.OrdinalIgnoreCase.Compare(PrereleaseVersion, other.PrereleaseVersion);

            if (result != 0)
            {
                return result;
            }

            return StringComparer.OrdinalIgnoreCase.Compare(OriginalText, other.OriginalText);
        }

        public bool Equals(SemanticVersion other)
        {
            return other != null && string.Equals(OriginalText, other.OriginalText, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SemanticVersion);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return OriginalText;
        }
    }
}
