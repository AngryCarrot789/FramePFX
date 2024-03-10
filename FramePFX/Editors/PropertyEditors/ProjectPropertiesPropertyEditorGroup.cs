//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors
{
    public class ProjectPropertiesPropertyEditorGroup : BasePropertyEditorGroup
    {
        private Project project;

        public Project Project
        {
            get => this.project;
            set
            {
                if (this.project == value)
                    return;
                this.project = value;
                this.ProjectChanged?.Invoke(this);
            }
        }

        public override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        public event BasePropertyEditorItemEventHandler ProjectChanged;

        public ProjectPropertiesPropertyEditorGroup() : base(typeof(Project))
        {
        }

        /// <summary>
        /// Recursively clears the state of all groups and editors
        /// </summary>
        public void ClearHierarchy()
        {
            if (!this.IsCurrentlyApplicable)
            {
                return;
            }

            if (this.Project != null)
            {
            }

            foreach (BasePropertyEditorObject obj in this.PropertyObjects)
            {
                switch (obj)
                {
                    case PropertyEditorSlot editor:
                        editor.ClearHandlers();
                        break;
                    case SimplePropertyEditorGroup group:
                        group.ClearHierarchy();
                        break;
                }
            }

            this.IsCurrentlyApplicable = false;
            this.Project = null;
        }

        public virtual void SetupHierarchyState(Project newProject)
        {
            if (ReferenceEquals(this.project, newProject))
            {
                return;
            }

            this.ClearHierarchy();
            if (newProject == null)
            {
                return;
            }

            // maybe calculate every possible type from the given input (scanning each object's hierarchy
            // and adding each type to a HashSet), and then using that to check for applicability.
            // It would probably be slower for single selections, which is most likely what will be used...
            // but the performance difference for multi select would make it worth it tbh

            List<Project> projectList = new List<Project>() {newProject};
            bool isApplicable = false;
            for (int i = 0, end = this.PropertyObjects.Count - 1; i <= end; i++)
            {
                BasePropertyEditorObject obj = this.PropertyObjects[i];
                if (obj is SimplePropertyEditorGroup group)
                {
                    group.SetupHierarchyState(projectList);
                    isApplicable |= group.IsCurrentlyApplicable;
                }
                else if (obj is PropertyEditorSlot editor)
                {
                    editor.SetHandlers(projectList);
                    isApplicable |= editor.IsCurrentlyApplicable;
                }
            }

            this.IsCurrentlyApplicable = isApplicable;
        }

        public override bool IsPropertyEditorObjectAcceptable(BasePropertyEditorObject obj)
        {
            return obj is PropertyEditorSlot || obj is BasePropertyEditorGroup;
        }
    }
}