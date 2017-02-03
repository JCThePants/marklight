﻿using UnityEngine;

namespace MarkLight
{   
    /// <summary>
    /// Generic base class for dependency view fields.
    /// </summary>
    public class ViewField<T> : ViewFieldBase
    {
        #region Fields

        [SerializeField]
        protected T _internalValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets view field notifying observers if the value has changed.
        /// </summary>
        public virtual T Value
        {
            get
            {
                if (ParentView != null && IsMapped)
                {
                    return (T)ParentView.GetValue(Path);
                }

                return _internalValue;
            }
            set
            {
                if (ParentView != null)
                {
                    ParentView.SetValue(Path, value);
                }
                else
                {
                    InternalValue = value;
                    _isSet = true;
                }                
            }
        }

        /// <summary>
        /// Gets or sets view field notifying observers if the value has changed.
        /// </summary>
        public virtual object ObjectValue
        {
            get
            {
                if (ParentView != null && IsMapped)
                {
                    return ParentView.GetValue(Path);
                }

                return _internalValue;
            }
            set
            {
                if (ParentView != null)
                {
                    ParentView.SetValue(Path, value);
                }
                else
                {
                    InternalValue = (T)value;
                    _isSet = true;
                }
            }
        }

        /// <summary>
        /// Sets view field directly without notifying observers that the value has changed.
        /// </summary>
        public virtual T DirectValue
        {
            set
            {
                if (ParentView != null && IsMapped)
                {
                    ParentView.SetValue(Path, value, true, null, null, false);
                }
                else
                {
                    _internalValue = value;
                    _isSet = true;
                }
            }
        }

        /// <summary>
        /// Sets view field directly without notifying observers that the value has changed.
        /// </summary>
        public virtual object DirectObjectValue
        {
            set
            {
                if (ParentView != null && IsMapped)
                {
                    ParentView.SetValue(Path, value, true, null, null, false);
                }
                else
                {
                    _internalValue = (T)value;
                    _isSet = true;
                }
            }
        }

        /// <summary>
        /// Gets boolean indicating if the value has been set. 
        /// </summary>
        public virtual bool IsSet
        {
            get
            {
                return ParentView != null ? ParentView.IsSet(Path) : _isSet;
            }
        }

        /// <summary>
        /// Gets or sets internal value without considering mappings and without notifying observers.
        /// </summary>
        public virtual T InternalValue
        {
            get
            {
                return _internalValue;
            }
            set
            {
                _internalValue = value;
                TriggerValueSet();
            }
        }

        #endregion
    }
}