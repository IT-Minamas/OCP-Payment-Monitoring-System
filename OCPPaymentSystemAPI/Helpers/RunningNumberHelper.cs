using OCPPaymentSystemAPI.Data;

namespace OCPPaymentSystemAPI.Helpers
{
    public class RunningNumberHelper
    {
        private readonly RunningNumberData _runningNumber;

        public RunningNumberHelper(RunningNumberData runningNumber)
        {
            _runningNumber = runningNumber;
        }

        public async Task<string> GenerateMemoNumberAsync()
        {
            return await _runningNumber.GenerateMemoNumberAsync();
        }
    }
}