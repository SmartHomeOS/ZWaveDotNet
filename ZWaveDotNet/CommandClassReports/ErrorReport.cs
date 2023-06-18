namespace ZWaveDotNet.CommandClassReports
{
    public class ErrorReport : ICommandClassReport
    {
        public readonly uint ErrorNumber;
        public readonly string ErrorMessage;

        public ErrorReport(uint errorNumber, string errorMessage)
        {
            this.ErrorNumber = errorNumber;
            this.ErrorMessage = errorMessage;
        }

        public override string ToString()
        {
            return $"Error {ErrorNumber}: {ErrorMessage}";
        }
    }
}
