namespace DigitalRiseModel.Converter
{
	internal class Options
	{
		public string InputFile { get; set; }
		public string OutputFile { get; set; }

		public bool GenerateTangentsAndBitangents { get; set; }
		public bool FlipWindingOrder { get; set; }

		public bool OverwriteModelFile { get; set; } = true;

		public override string ToString() =>
			$"Input={InputFile}, Output={OutputFile}, GenerateTangents={GenerateTangentsAndBitangents}, FlipWindingOrder={FlipWindingOrder},\n" +
			$"OverwriteModelFile={OverwriteModelFile}";
	}
}
