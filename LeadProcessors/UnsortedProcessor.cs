using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class UnsortedProcessor : AbstractLeadProcessor, ILeadProcessor
    {
        #region Definition
        private readonly string _uid;

        public UnsortedProcessor(string uid, AmoAccount acc, TaskList processQueue, Log log, CancellationToken token)       //Процессор принимает сделку из Неразобранного
            : base(acc, processQueue, log, token)
        {
            _uid = uid;
        }
        #endregion

        #region Realization
        public override Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_uid);
                return Task.FromCanceled(_token);
            }
            try
            {
                _leadRepo.AcceptUnsorted(_uid);                                                                             //принимаем из Неразобранного по uid
                _processQueue.Remove(_uid);
                _log.Add($"Unsorted accepted: {_uid}");
                return Task.CompletedTask;
            }
            catch (Exception e) 
            {
                _processQueue.Remove(_uid);
                _log.Add($"Error: Unable to accept unsorted: {_uid}:{e.Message}");
                return Task.FromException(e);
            }
        }
        #endregion
    }
}