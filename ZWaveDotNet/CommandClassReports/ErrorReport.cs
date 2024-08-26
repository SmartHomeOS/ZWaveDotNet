// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
