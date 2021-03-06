﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qoollo.Turbo.IoC.Helpers;
using Qoollo.Turbo.IoC.ServiceStuff;

namespace Qoollo.Turbo.IoC.Lifetime
{
    /// <summary>
    /// Lifetime container that holds a single instance of an object. 
    /// That instance is created lazily on the first call of 'GetInstance' method.
    /// </summary>
    public class DeferedSingletonLifetime: LifetimeBase
    {
        private readonly Func<IInjectionResolver, object> _createInstanceFunc;
        private volatile object _obj;
        private readonly object _lockObj = new object();
        private volatile bool _isInited;


        /// <summary>
        /// DeferedSingletonLifetime constructor
        /// </summary>
        /// <param name="createInstanceFunc">Instance creation method</param>
        /// <param name="objType">The type of the object to be stored in the current Lifetime container</param>
        public DeferedSingletonLifetime(Func<IInjectionResolver, object> createInstanceFunc, Type objType)
            : base(objType)
        {
            if (createInstanceFunc == null)
                throw new ArgumentNullException(nameof(createInstanceFunc));

            _isInited = false;
            _createInstanceFunc = createInstanceFunc;
        }


        /// <summary>
        /// Core method to create an instance of an object (separated to improve the performance)
        /// </summary>
        /// <param name="resolver">Injection resolver to acquire parameters</param>
        private void CreateInstanceCore(IInjectionResolver resolver)
        {
            lock (_lockObj)
            {
                if (!_isInited)
                {
                    _obj = _createInstanceFunc(resolver);
                    _isInited = true;
                }
            }
        }

        /// <summary>
        /// Resolves the object held by the container
        /// </summary>
        /// <param name="resolver">Injection resolver to acquire parameters</param>
        /// <returns>Resolved instance of the object</returns>
        /// <exception cref="CommonIoCException">Can be raised when injections not found</exception>
        public sealed override object GetInstance(IInjectionResolver resolver)
        {
            TurboContract.Requires(resolver != null, conditionString: "resolver != null");

            if (!_isInited)
                CreateInstanceCore(resolver);

            return _obj;
        }

        /// <summary>
        /// Cleans-up all resources
        /// </summary>
        /// <param name="isUserCall">True when called explicitly by user from Dispose method</param>
        protected override void Dispose(bool isUserCall)
        {
            if (_obj is IDisposable objDisp)
                objDisp.Dispose();

            base.Dispose(isUserCall);
        }
    }
}
