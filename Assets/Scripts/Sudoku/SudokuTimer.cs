using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sudoku {
    public class SudokuTimer {
        public bool   IsPaused      { get; private set; }
        public bool  IsStopped     { get; private set; }
        public float  Seconds       { get; private set; }

        public event System.Action<float> OnTimerUpdated;

        CancellationToken _token;

        public void StartTimer(CancellationToken tkn) {
            _token = tkn;
            Seconds = 0;
            IsPaused = false;
            IsStopped = false;
            UpdateTimer().Forget();
        }

        async UniTaskVoid UpdateTimer() {
            var previousTime = 0;
            OnTimerUpdated?.Invoke(Seconds);
            while (_token.IsCancellationRequested == false && !IsStopped) {
                if (IsPaused) {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    continue;
                }

                previousTime = (int)Seconds;
                await UniTask.Yield(PlayerLoopTiming.Update);
                Seconds += Time.deltaTime;
                if (previousTime != (int)Seconds) OnTimerUpdated?.Invoke(Seconds);
            }
        }

        public void PauseTimer()  => IsPaused = true;
        public void ResumeTimer() => IsPaused = false;
        public void StopTimer()   => IsStopped = true;
    }
}