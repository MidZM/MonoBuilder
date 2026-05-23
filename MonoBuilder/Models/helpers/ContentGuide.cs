using System;
using System.Collections.Generic;
using System.Text;

namespace MonoBuilder.Models.helpers
{
	public class ContentGuide
	{
		public string SystemName { get; }
		public string GuideStart { get; }
		public string GuideEnd { get; }

		public ContentGuide(string systemName, string guideStart, string guideEnd)
		{
			SystemName = systemName;
			GuideStart = guideStart;
			GuideEnd = guideEnd;
		}
	}
}
