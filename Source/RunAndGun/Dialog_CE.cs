using HugsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.Steam;

namespace RunAndGun
{
    public class Dialog_CE : Window
    {
        private string ConfirmButtonCaption = "RG_Dialog_CE_ConfirmButton".Translate();
        private readonly Color ConfirmButtonColor = Color.green;
        private readonly Vector2 ConfirmButtonSize = new Vector2(180, 40);

        private readonly string title;
        private readonly string message;
        private bool closedLogWindow;

        public override Vector2 InitialSize
        {
            get { return new Vector2(500f, 400f); }
        }

        public Dialog_CE(string title, string message)
        {
            this.title = title;
            this.message = message;
            closeOnCancel = true;
            doCloseButton = false;
            doCloseX = false;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override void PostClose()
        {
            base.PostClose();
            if (closedLogWindow)
            {
                EditWindow_Log.wantsToOpen = true;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            var logWindow = Find.WindowStack.WindowOfType<EditWindow_Log>();
            if (logWindow != null)
            {
                // hide the log window while we are open
                logWindow.Close(false);
                closedLogWindow = true;
            }
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(inRect.x, inRect.y, inRect.width, 40);
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(inRect.x, inRect.y + titleRect.height, inRect.width, inRect.height - ConfirmButtonSize.y - titleRect.height), message);
            Rect closeButtonRect;
            var prevColor = GUI.color;
            GUI.color = ConfirmButtonColor;
            var downloadButtonRect = new Rect(inRect.x, inRect.height - ConfirmButtonSize.y, ConfirmButtonSize.x, ConfirmButtonSize.y);
            if (Widgets.ButtonText(downloadButtonRect, ConfirmButtonCaption))
            {
                Base.Instance.ResetForbidden();
                Close();
                Base.dialogCEShown.Value = true;
                HugsLibController.SettingsManager.SaveChanges();
            }
            GUI.color = prevColor;
            closeButtonRect = new Rect(inRect.width - CloseButSize.x, inRect.height - CloseButSize.y, CloseButSize.x, CloseButSize.y);
            if (Widgets.ButtonText(closeButtonRect, "RG_Dialog_CE_DisallowButton".Translate()))
            {
                Close();
                Base.dialogCEShown.Value = true;
                HugsLibController.SettingsManager.SaveChanges();
            }
        }
    }
}