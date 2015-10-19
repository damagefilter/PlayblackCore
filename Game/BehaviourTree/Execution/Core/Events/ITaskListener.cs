using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.BehaviourTree.Execution.Core.Events {
    public interface ITaskListener {

        /// <summary>
        /// Called when an important change in the status of a task has taken place.
        /// </summary>
        /// <param name="?"></param>
        void OnChildStatusChanged(TaskEvent e);
    }
}
