using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reinforcement
{
    public class WarningSuppressor : IFailuresPreprocessor
    {
        FailureProcessingResult IFailuresPreprocessor.PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            String transactionName = failuresAccessor.GetTransactionName();

            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();


            if (fmas.Count == 0)
                return FailureProcessingResult.Continue;


            if (transactionName.Equals("EXEMPLE"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    if (fma.GetSeverity() == FailureSeverity.Error)
                    {
                        failuresAccessor.DeleteAllWarnings();
                        return FailureProcessingResult.ProceedWithRollBack;
                    }
                    else
                    {
                        failuresAccessor.DeleteWarning(fma);
                    }

                }
            }
            else
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    failuresAccessor.DeleteAllWarnings();
                }
            }
            return FailureProcessingResult.Continue;
        }
    }
}
