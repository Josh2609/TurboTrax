using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScore {

        private static List<Action> _scoreActions;
        public static void RegisterForScoreChange(Action actionToRegister)
        {
            // Lazily instantiate the list, in case nothing ever registers
            if (_scoreActions == null)
            {
                _scoreActions = new List<Action>();
            }

            _scoreActions.Add(actionToRegister);
        }
        public static void DeregisterForScoreChange(Action actionToDeregister)
        {
            if (_scoreActions.Contains(actionToDeregister))
            {
                _scoreActions.Remove(actionToDeregister);
            }
        }
        private static void CallRegisteredScoreFunctions()
        {
            if (_scoreActions != null)
            {
                foreach (Action action in _scoreActions)
                {
                    action();
                }
            }
        }
 
}
