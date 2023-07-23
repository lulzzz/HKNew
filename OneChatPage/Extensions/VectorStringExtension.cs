namespace OneChatPage.Extensions
{
    public class VStr
    {
        public static List<float> StrToVec(string str)
        {
			//tr string to vector[]
            var StrToVec = str.Split(',').Select(x => float.Parse(x)).ToList();
            return StrToVec;
		}

        public static string VecToStr(float[] vec)
        {
            	//tr vector[] to string
			var VecToStr = string.Join(",", vec);
			return VecToStr;
        }
    }
}
