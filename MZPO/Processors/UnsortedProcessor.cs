using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Threading;

namespace MZPO.Processors
{
    public class UnsortedProcessor
    {
        #region Definition
        protected readonly IBaseRepo<Lead> _leadRepo;
        protected readonly AmoAccount _acc;
        protected readonly TaskList _processQueue;
        protected readonly CancellationToken _token; private readonly string _uid;

        public UnsortedProcessor(string uid, AmoAccount acc, TaskList processQueue, CancellationToken token)         //Процессор принимает сделку из Неразобранного
        {
            _acc = acc;
            _leadRepo = acc.GetRepo<Lead>();
            _processQueue = processQueue;
            _token = token;
            _uid = uid;
        }
        #endregion

        #region Realization
        public void Run()
        {
            if (_token.IsCancellationRequested) return;
            try
            {
                _leadRepo.AcceptUnsorted(_uid);                                                                         //принимаем из Неразобранного по uid
                _processQueue.Remove(_uid);
                Log.Add($"Unsorted accepted: {_uid}");
            }
            catch (Exception e) 
            {
                _processQueue.Remove(_uid);
                Log.Add($"Error: Unable to accept unsorted: {_uid}:{e.Message}");
            }
        }
        #endregion
    }
}
