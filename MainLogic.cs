using System;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Session;
using Torch.Managers.PatchManager;
using Torch.Session;

namespace MESInhibitorInhibitor
{
    public class MainLogic : TorchPluginBase
    {
        private static readonly Logger Log = LogManager.GetLogger("[MESInhibitorInhibitor]: Plugin");
        private TorchSessionManager _sessionManager;

        public override void Init(ITorchBase torch) {
            base.Init(torch);
            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (_sessionManager != null)
                _sessionManager.SessionStateChanged += OnSessionStateChanged;
        }
        
        private void OnSessionStateChanged(ITorchSession session, TorchSessionState state)
        {
            if (state != TorchSessionState.Loaded) return;
            try {
                var patchMgr = Torch.Managers.GetManager<PatchManager>();
                var ctx = patchMgr.AcquireContext();
                PatchMesInhibitors.Patch(ctx);
                patchMgr.Commit();
            }
            catch (Exception e) {
                Log.Error(e, "Failed to apply MESInhibitorInhibitor patches.");
            }
        }
    }
}
