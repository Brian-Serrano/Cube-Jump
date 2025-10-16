using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPV5Manager
{
    private static IAPV5Manager instance;

    private StoreController _store;
    private readonly Dictionary<string, Product> _productsById = new();
    private Dictionary<string, Action<string>> _pendingGrants = new();
    private Dictionary<string, Action> _failedGrants = new();

    public static IAPV5Manager GetInstance()
    {
        instance ??= new IAPV5Manager();

        return instance;
    }

    private IAPV5Manager()
    {
        Initialize();
    }

    private async void Initialize()
    {
        await InitializeIAP();
    }

    public async Task InitializeIAP()
    {
        try
        {
            // optional, but helps avoid warnings in some projects
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            _store = UnityIAPServices.StoreController();

            // subscribe to v5 lifecycle events
            _store.OnProductsFetched += OnProductsFetched;
            _store.OnProductsFetchFailed += OnProductsFetchFailed;
            _store.OnPurchasePending += OnPurchasePending;       // grant here, then ConfirmPurchase
            _store.OnPurchaseFailed += OnPurchaseFailed;
            _store.OnPurchaseConfirmed += OnPurchaseConfirmed;   // after ConfirmPurchase succeeds
            _store.OnPurchaseDeferred += OnPurchaseDeferred;
            _store.OnPurchasesFetched += OnPurchasesFetched;     // restore / cold-start reconciliation
            _store.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            _store.OnStoreDisconnected += OnStoreDisconnected;

            // connect to the platform store (Google Play / App Store, etc.)
            await _store.Connect();

            // build a CatalogProvider from your IAP Catalog and fetch products
            var catalogProvider = BuildCatalogProviderFromIapCatalog();
            catalogProvider.FetchProducts(defs => _store.FetchProducts(defs));

            // (optional) recover any existing transactions on cold start
            _store.ProcessPendingOrdersOnPurchasesFetched(true);
            _store.FetchPurchases();
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] Initialize failed: {e}");
        }
    }

    // --------- UI entry points ---------

    public void Buy(string productId, Action<string> grantEntitlement, Action failedGrant)
    {
        if (_store == null)
        {
            Debug.LogWarning("[IAP] Store not ready yet.");
            return;
        }

        _pendingGrants[productId] = grantEntitlement;
        _failedGrants[productId] = failedGrant;
        _store.PurchaseProduct(productId);
    }

    // iOS-style “Restore Purchases” button; harmless on Android
    public void RestorePurchases()
    {
        if (_store == null) return;

        _store.RestoreTransactions((success, error) =>
        {
            Debug.Log($"[IAP] RestoreTransactions => success:{success} error:{error}");
        });

        _store.ProcessPendingOrdersOnPurchasesFetched(true);
        _store.FetchPurchases();
    }

    // --------- Events (v5 flow) ---------

    private void OnProductsFetched(List<Product> products)
    {
        _productsById.Clear();
        foreach (var p in products)
        {
            _productsById[p.definition.id] = p;
            Debug.Log($"[IAP] Product fetched: {p.definition.id} | {p.metadata.localizedTitle} | {p.metadata.localizedPriceString}");
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogError($"[IAP] Product fetch failed: reason={failure.FailureReason}");
    }

    // Called when a purchase arrives from the store.
    // Grant the entitlement(s) here, then ConfirmPurchase.
    private void OnPurchasePending(PendingOrder order)
    {
        foreach (var line in order.CartOrdered.Items())
        {
            var productId = line.Product.definition.id;
            _pendingGrants[productId]?.Invoke(productId);
        }

        // if you do server receipt validation, send order.Info to your server first,
        // then call ConfirmPurchase only after server says it's valid.
        _store.ConfirmPurchase(order);
    }

    private void OnPurchaseFailed(FailedOrder failure)
    {
        Debug.LogWarning($"[IAP] Purchase failed: reason={failure.FailureReason}");

        foreach (var line in failure.CartOrdered.Items())
        {
            var productId = line.Product.definition.id;
            _failedGrants[productId]?.Invoke();
        }
    }

    private void OnPurchaseConfirmed(Order order)
    {
        Debug.Log($"[IAP] Purchase confirmed: tx={order.Info.TransactionID}");
    }

    private void OnPurchaseDeferred(DeferredOrder order)
    {
        Debug.Log("[IAP] Purchase deferred (awaiting approval).");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        Debug.Log($"[IAP] Purchases fetched: pending={orders.PendingOrders.Count} confirmed={orders.ConfirmedOrders.Count} deferred={orders.DeferredOrders.Count}");
        // you can re-grant non-consumables here if needed, based on your saved state
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        Debug.LogWarning($"[IAP] Purchases fetch failed: {failure.Message}");
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        Debug.LogWarning($"[IAP] Store disconnected: {failure.Message}");
        // consider retrying Connect() with your own policy
    }

    // --------- Helpers ---------

    // Builds a CatalogProvider using the products you set in the IAP Catalog (Window/Unity IAP/IAP Catalog).
    private static CatalogProvider BuildCatalogProviderFromIapCatalog()
    {
        var provider = new CatalogProvider();

        var catalog = ProductCatalog.LoadDefaultCatalog();
        if (catalog == null || catalog.allProducts == null) return provider;

        foreach (var item in catalog.allProducts)
        {
            StoreSpecificIds storeIds = null;
            if (item.allStoreIDs != null && item.allStoreIDs.Any())
            {
                storeIds = new StoreSpecificIds();
                foreach (var sid in item.allStoreIDs)
                {
                    // examples of sid.store: "GooglePlay", "AppleAppStore"
                    storeIds.Add(sid.store, sid.id);
                }
            }

            provider.AddProduct(item.id, item.type, storeIds);
        }

        return provider;
    }
}