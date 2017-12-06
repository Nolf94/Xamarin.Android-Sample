using System;
using Android.App;
using Android.Content;

namespace WPC_Android.AppLayer
{
    //Class that manages the login session and the session logout of the accounts saving data in shared preferences on the device
    class SessionControllerFactory
    {

        //-------------------------------------------------------------------------//
        //                             Data                                        //        
        //-------------------------------------------------------------------------//
        private string  _sessionNumber;

        private Context _context;

        private ISharedPreferences _session;

        private  static string _accountSessionPref = "accountPrefs";

        private static SessionControllerFactory _instance;

        //-------------------------------------------------------------------------//
        //                             Constructors                                //        
        //-------------------------------------------------------------------------//
        public SessionControllerFactory(Context context)
        {
            _context = context;
            _session = _context.GetSharedPreferences(_accountSessionPref, FileCreationMode.Private);
        }

        //-------------------------------------------------------------------------//


        //-------------------------------------------------------------------------//
        //                             Main Functions                              //        
        //-------------------------------------------------------------------------//
        //Get the instance or create if not exist
        public static SessionControllerFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SessionControllerFactory(Application.Context);
                }

                return _instance;
            }
        }
        //-------------------------------------------------------------------------//
        //Function to cancel the current account's session
        public void Logout()
        {
            ISharedPreferencesEditor editor = _session.Edit();
            editor.Clear();
            editor.Commit();
        }

        //-------------------------------------------------------------------------//

        //Function for save the current account's session
        public void SaveLoginSession(string number)
        {
            _sessionNumber = number;
            ISharedPreferencesEditor session_editor = _session.Edit();
            session_editor.PutString("number", _sessionNumber);
            session_editor.Commit();
        }

        //------------------------------------------------------------s-------------//

        //Function for check the current accounts's number
        public bool CheckCredentials()
        {
            ISharedPreferences preferences = _context.GetSharedPreferences(_accountSessionPref, FileCreationMode.Private);
            string number = preferences.GetString("number", "");
            if (!number.Equals(""))
            {
                return true;
            }
            return false;
        }

        //-------------------------------------------------------------------------//

        public string GetNumberSession()
        {
            ISharedPreferences preferences = _context.GetSharedPreferences(_accountSessionPref, FileCreationMode.Private);
            String number = preferences.GetString("number", "");
            return number;
        }
    }

    //-------------------------------------------------------------------------//
}