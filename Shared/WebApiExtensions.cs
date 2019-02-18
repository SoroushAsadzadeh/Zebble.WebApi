﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Zebble
{
    public static class WebApiExtensions
    {
        public static async Task SetFromApi<TViewModel, TApiResult>(
         this Bindable<TViewModel> @this,
         Expression<Func<TViewModel, TApiResult>> viewModelProperty,
         string apiUrl,
         AsyncEvent<RevisitingEventArgs> pageRevisiting = null)
        {
            var binding = new Bindable<TViewModel>.ViewModelMemberBinding<TApiResult>(@this, viewModelProperty);

            Task refresh()
            {
                return BaseApi.Get<TApiResult>(apiUrl, cacheChoice: ApiResponseCache.PreferThenUpdate, refresher: binding.Update);
            }

            // If there is a cached result already available, set it immediately
            var result = await BaseApi.Get<TApiResult>(apiUrl, cacheChoice: ApiResponseCache.CacheOrNull);
            if (result != null) await binding.Update(result);

            // Otherwise send a fresh request and apply the value.
            refresh().RunInParallel();

            // When gone back to the page, also refresh.
            if (pageRevisiting != null) pageRevisiting.Handle(x => refresh());
        }
    }
}