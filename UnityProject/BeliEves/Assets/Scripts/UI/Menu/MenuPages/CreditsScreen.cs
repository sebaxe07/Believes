using System.Collections;
using System.Collections.Generic;
using UI.Menu;
using UnityEngine;

namespace MenuManagement
{
    public class CreditsScreen : Menu<CreditsScreen>
    {
        public override void OnBackPressed()
        {
            base.OnBackPressed();
                
            // custom behavior ,..
        }
    }
}
