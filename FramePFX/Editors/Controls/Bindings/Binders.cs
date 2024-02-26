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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Windows;
using FramePFX.Utils.Accessing;

namespace FramePFX.Editors.Controls.Bindings {
    public static class Binders {
        public static GetSetAutoEventPropertyBinder<TModel> AccessorAEDP<TModel, TValue>(DependencyProperty property, string eventName, string propertyOrFieldName) where TModel : class {
            // Uses cached accessor
            return AccessorAEDP<TModel, TValue>(property, eventName, ValueAccessors.LinqExpression<TValue>(typeof(TModel), propertyOrFieldName, true));
        }

        public static GetSetAutoEventPropertyBinder<TModel> AccessorAEDP<TModel, TValue>(DependencyProperty property, string eventName, ValueAccessor<TValue> accessor) where TModel : class {
            return new GetSetAutoEventPropertyBinder<TModel>(property, eventName, b => accessor.GetObjectValue(b.Model), (b, v) => accessor.SetObjectValue(b.Model, v));
        }
    }
}