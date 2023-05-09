using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StoreController : MonoBehaviour
{
    Canvas uiBalance;
    StockManager sm;

    [SerializeField] int level = 1;
    float balance;
    int maxStock;
    int currentStock = 0;
    public int step, phase;
    float timer = 0f;
    public bool isAI = false;
    public bool isSelling = true;

    Dictionary<string, Product> products = new Dictionary<string, Product>();
    List<CustomerController> queue = new List<CustomerController>();
    public static object _QueueLock = new object();

    Dictionary<string, int> prodToITbeta = new Dictionary<string, int>
    {
        { StockManager.prodNames[0], 1 },
        { StockManager.prodNames[1], 1 },
        { StockManager.prodNames[2], 1 },
        { StockManager.prodNames[3], 1 },
        { StockManager.prodNames[4], 1 }
    };

    ConcurrentDictionary<string, int> soldProducts = new ConcurrentDictionary<string, int>();

    TextMeshProUGUI cash, apple, shirt, phone, gpu, rolex; // UI Amounts

    public void Awake()
    {
        // validate init after reset
        if (phase == 2)
        {
            level = 1;
            step = 0;
            currentStock = 0;
            queue.Clear();
            products.Clear();
            prodToITbeta.Keys.ToList().ForEach(key => prodToITbeta[key] = 1);
            soldProducts.Keys.ToList().ForEach(key => soldProducts[key] = 0);
        }

        if (phase == 0 || (phase == 1 && isAI))
            level = 1;

        InitUiTMPs();
        uiBalance = GetComponentInChildren<Canvas>();
        UpdateModel();
        balance = StoreParams.STORE_STARTING_BALANCE;
        InitStock();
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
        InitPricesAndIT();
        UpdateUIPrices(); // inital price set
        if (level < 2)
        {
            phone.transform.parent.gameObject.SetActive(false);
            gpu.transform.parent.gameObject.SetActive(false);
        }
        if (level < 3)
            rolex.transform.parent.gameObject.SetActive(false);

        //PrintShop();
    }

    private void InitStock()
    {
        switch (level)
        {
            case 1: maxStock = StoreParams.STOCK_LEVEL_ONE; break;
            case 2: maxStock = StoreParams.STOCK_LEVEL_TWO; break;
            case 3: maxStock = StoreParams.STOCK_LEVEL_THREE; break;
        }
        currentStock = 0;
    }

    private void InitUiTMPs()
    {
        var uiAmounts = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        apple = uiAmounts[0];
        shirt = uiAmounts[1];
        phone = uiAmounts[2];
        gpu = uiAmounts[3];
        rolex = uiAmounts[4];
        cash = uiAmounts[5];
    }

    public void Update()
    {

        timer += Time.deltaTime;

        if (timer >= 0.5f)
        {
            UpdateUIPrices();
            timer = 0;
        }

        // make the text face the camera
        cash.text = "" + balance.ToString("#.##") + "$";
        uiBalance.transform.LookAt(Camera.main.transform.position);
        uiBalance.transform.Rotate(new Vector3(0, 180, 0));

        if ((!isAI || isSelling) && SafeDequeue(out CustomerController cc))
        {
            Transaction(cc);
            if (isAI)
            {
                if (phase == 1)
                    Destroy(cc.gameObject);
                step++;
                if (step != 0 && step % MLParams.Transaction_Delta == 0)
                    isSelling = false;
            }
        }
    }

    private void UpdateModel()
    {
        GetComponent<Animator>().Play("StorePop", 0);

        //delete old model
        foreach (var g in GetComponentsInChildren<Transform>())
            if (g.tag == "StoreModel")
                Destroy(g.gameObject);

        GameObject model = null;
        switch (level)
        {
            case 0:
                model = Instantiate(Resources.Load("Trash"), transform.position, transform.rotation, transform) as GameObject;
                uiBalance.enabled = false;
                break;
            case 1:
                model = Instantiate(Resources.Load("Stand"), transform.position, transform.rotation, transform) as GameObject;
                model.transform.localPosition = new Vector3(0, 0.3f);
                uiBalance.enabled = true;
                break;
            case 2:
                model = Instantiate(Resources.Load("JapStore"), transform.position, transform.rotation, transform) as GameObject;
                model.transform.localPosition = new Vector3(0, -0.15f);
                break;
            case 3:
                model = Instantiate(Resources.Load("Arcade"), transform.position, transform.rotation, transform) as GameObject;
                model.transform.localPosition = new Vector3(0, -0.2f);
                break;
        }

        GetComponentInChildren<Canvas>().transform.localPosition = new Vector3(0, GetComponentInChildren<Renderer>().bounds.max.y * level * 3, 0);
    }

    private void UpdateUIPrices()
    {
        apple.text = products.TryGetValue("Apple", out Product val) ? "" + val.amount : "0";
        shirt.text = products.TryGetValue("Shirt", out Product val2) ? "" + val2.amount : "0";
        phone.text = products.TryGetValue("Phone", out Product val3) ? "" + val3.amount : "0";
        gpu.text = products.TryGetValue("GPU", out Product val4) ? "" + val4.amount : "0";
        rolex.text = products.TryGetValue("Rolex", out Product val5) ? "" + val5.amount : "0";
    }

    public float GetBalance()
    {
        return balance;
    }

    public void SetLevel(int level, bool payFromBalance = true)
    {
        this.level = level;
        UpdateModel();
        if (level == 1)
        {
            phone.transform.parent.gameObject.SetActive(false);
            gpu.transform.parent.gameObject.SetActive(false);
            rolex.transform.parent.gameObject.SetActive(false);
            maxStock = StoreParams.STOCK_LEVEL_ONE;
        }
        else if (level == 2)
        {
            phone.transform.parent.gameObject.SetActive(true);
            gpu.transform.parent.gameObject.SetActive(true);
            rolex.transform.parent.gameObject.SetActive(false);
            maxStock = StoreParams.STOCK_LEVEL_TWO;
            if (payFromBalance)
                balance -= StoreParams.UPGRADE_LEVEL_TWO_PRICE;
        }
        else if (level == 3)
        {
            rolex.transform.parent.gameObject.SetActive(true);
            maxStock = StoreParams.STOCK_LEVEL_THREE;
            if (payFromBalance)
                balance -= StoreParams.UPGRADE_LEVEL_THREE_PRICE;
        }
    }

    public int GetLevel()
    {
        return level;
    }

    public void SafeEnqueue(CustomerController cc)
    {
        lock (_QueueLock)
        {
            queue.Add(cc);
        }
    }

    public bool SafeDequeue(out CustomerController cc)
    {
        lock (_QueueLock)
        {
            if (queue.Count > 0)
            {
                cc = queue[0];
                queue.Remove(queue[0]);
                return true;
            }
            else
            {
                cc = null;
                return false;
            }
        }
    }

    private void PrintShop()
    {
        StringBuilder sb = new();
        if (products.Count > 0)
            sb.Append("[S]" + name + " ");
        foreach (Product p in products.Values)
            sb.Append(p + ", ");
        sb.Append("Balance: " + balance);

        if (!string.IsNullOrEmpty(sb.ToString()))
            Debug.Log(sb.ToString());
    }

    public void Restock()
    {
        if (balance > 0)
        {
            foreach (string prodName in sm.GetBuyList(level))
                BuyProduct(new Product(prodName, GetRestockAmount(prodName)));
        }
    }

    private int GetRestockAmount(string prodName)
    {
        int total_IT = 0;
        products.Values.ToList().ForEach(p => total_IT += p.Invest_tend);

        if (total_IT == 0)
            if (products.Count > 0)
                return (int)((maxStock - currentStock) / products.Count); // even amount of products
            else
                total_IT = maxStock;

        if (products.TryGetValue(prodName, out Product p))
            return (int)Mathf.Round((float)p.Invest_tend / total_IT * (maxStock - currentStock));

        return (int)Mathf.Round((float)Random.Range(1, 11) / total_IT * (maxStock - currentStock)); // buy a random amount
    }


    private bool IsStockEmpty()
    {
        return products.ToListPooled().TrueForAll(kvp => kvp.Value.amount == 0);
    }

    public void Tax(int TAX)
    {
        int finalTax = TAX * level * level;
        foreach (Product product in products.Values.Where(p => p.amount > 0))
            finalTax += sm.GetProductTax(product.name) * product.amount;
        balance -= finalTax;

        if (balance < 0)
        {
            if (level == 1)
            {
                products.ToList().ForEach(kvp => kvp.Value.amount = 0);
                var pfm = GameObject.Find("SimulationController").GetComponent<PathfindingManager>();
                if (phase != 1) // phase 1 doesn't use this script
                    pfm.SafeRemove(gameObject);
                else
                    SetLevel(0, false);
            }
            else if (level == 2)
            {
                balance += StoreParams.UPGRADE_LEVEL_TWO_PRICE / 2f;
                products.ToList().ForEach(kvp => kvp.Value.amount = 0);
                SetLevel(--level, false);
            }
            else if (level == 3)
            {
                balance += StoreParams.UPGRADE_LEVEL_THREE_PRICE / 2f;
                products.ToList().ForEach(kvp => kvp.Value.amount = 0);
                SetLevel(--level, false);
            }
            currentStock = 0;
        }

        // Because taxations only happens at the end of a workday we can normalize IT here
        NormalizeIT();
    }

    private void NormalizeIT()
    {
        int totalIT = 0;
        products.ToList().ForEach(kvp => totalIT += kvp.Value.Invest_tend);

        // validate non-zero
        if (totalIT == 0)
            return;

        foreach (var kvp in products)
        {
            products[kvp.Key].Invest_tend = Mathf.RoundToInt(products[kvp.Key].Invest_tend / totalIT * maxStock);
        }
    }

    public float GetTotalTax()
    {
        int finalTax = StoreParams.BASE_TAX * level * level;
        foreach (Product product in products.Values.Where(p => p.amount > 0))
            finalTax += sm.GetProductTax(product.name) * product.amount;
        return finalTax;
    }

    public void Transaction(CustomerController cc)
    {
        bool hasBoughtSomething = false;
        Dictionary<string, bool> hasLooked = new Dictionary<string, bool>();
        foreach (string prodName in StockManager.prodNames)
            hasLooked.Add(prodName, false);

        foreach (Product p in cc.GetProducts())
        {
            SellProduct(p, cc, ref hasBoughtSomething);
            hasLooked[p.name] = true;
            prodToITbeta[p.name] = 1;
        }

        // interate over all the products that were overlooked and exist in the shop and decrease IT.
        if (!isAI)
            foreach (KeyValuePair<string, bool> kvp in hasLooked.Where(kvp => !kvp.Value))
                if (products.TryGetValue(kvp.Key, out Product p))
                {
                    p.Invest_tend -= prodToITbeta[p.name];
                    prodToITbeta[p.name]++;
                }

        if (GameObject.Find("SimulationController").GetComponent<CustomerManager>() != null)
            cc.CompleteTransaction(hasBoughtSomething);
    }

    public void InitialRandomOffsetPrice(Product product, int alpha)
    {
        float price = sm.GetMaxPrice(product.name) * (StoreParams.INITIAL_PRECENTILE); // lower quarter
        float offset = sm.GetMaxPrice(product.name) * Random.Range(0, alpha) / 100f;
        product.Price = price + offset;
    }

    public Dictionary<string, float> GetProductPrices()
    {
        Dictionary<string, float> temp = new Dictionary<string, float>();
        foreach (KeyValuePair<string, Product> kvp in products)
            temp.Add(kvp.Key, kvp.Value.Price);
        return temp;
    }

    void InitPricesAndIT()
    {
        //clear products
        products.Clear();

        foreach (string prodName in sm.GetBuyList(level))
            BuyProduct(new Product(prodName, GetRestockAmount(prodName)));
    }

    public Dictionary<string, Product> GetProducts()
    {
        return products;
    }

    /// <summary>
    /// Update balance and add new products to the list,
    /// if they don't exist initiallize them.
    /// </summary>
    public void BuyProduct(Product product)
    {
        float price = sm.GetProductionPrice(product.name);
        if (products.TryGetValue(product.name, out Product existingProd))
        {
            if (balance - product.amount * price < 0)
                product.amount = Mathf.FloorToInt((balance > 0 ? balance : 1) / price);
            balance -= product.amount * price;
            existingProd.amount += product.amount;
        }
        else
        {
            InitialRandomOffsetPrice(product, StoreParams.PRICE_OFFSET);
            if (balance - product.amount * price < 0)
                product.amount = Mathf.FloorToInt((balance > 0 ? balance : 1) / price);
            balance -= product.amount * price;
            products.Add(product.name, product);
        }
        currentStock += product.amount;
    }

    /// <summary>
    /// Takes in a product that its values represent:
    ///     name - product name.
    ///     amount - how many to sell.
    ///     price / invest_rate - change for a transaction
    ///  returns the amount sold.
    /// </summary>
    public void SellProduct(Product product, CustomerController cc, ref bool hasBoughtSomething)
    {
        // AIStore Selling Logic
        if (isAI)
        {
            if (products.TryGetValue(product.name, out Product existingProdAI) && existingProdAI.amount > 0)
            {
                int sold = 0;
                if (existingProdAI.amount < product.amount)
                    sold = existingProdAI.amount;
                else
                    sold = product.amount;

                if (existingProdAI.Price <= product.Price)
                {
                    balance += sold * existingProdAI.Price;
                    currentStock -= sold;
                    existingProdAI.amount -= sold;
                }
                if (!soldProducts.TryAdd(product.name, sold))
                    soldProducts[product.name] += sold;
            }
            return; // finish here for AIStore
        }

        // Regular store logic
        float price_delta =
            GameObject.Find("SimulationController").GetComponent<StockManager>().GetMaxPrice(product.name)
            * (StoreParams.PRICE_DELTA_BASE + Random.Range(StoreParams.PRICE_DELTA_LOWER_BOUND, StoreParams.PRICE_DELTA_UPPER_BOUND)); // see in MarketParams for further info

        if (products.TryGetValue(product.name, out Product existingProd) && existingProd.amount > 0)
        {
            int sold = 0;
            if (existingProd.amount < product.amount)
                sold = existingProd.amount;
            else
                sold = product.amount;

            if (existingProd.Price <= product.Price)
            {
                balance += sold * existingProd.Price;
                currentStock -= sold;

                existingProd.Price += price_delta * sold;   // shops more interested in selling
                existingProd.amount -= sold;
                product.amount -= sold;
                product.Price -= price_delta * sold;    // customers less interested in buying

                if (!hasBoughtSomething && sold != 0)
                    hasBoughtSomething = true;
            }
            else // price too high
            {
                // Store changes
                float bankruptPanic = Mathf.Clamp(existingProd.Price / (balance - GetTotalTax() > 0 ? balance : 1),
                    StoreParams.BANKRUPT_LOWER_BOUND, StoreParams.BANKRUPT_UPPER_BOUND * existingProd.Price); // see MarketParams for further info
                existingProd.Price -= price_delta + bankruptPanic; // 10% + delta + (20% to epsilon)

                // Customer changes
                int ttl = cc.ttl; // this is used for a race condition
                if (ttl == 0)
                    ttl = 1;
                product.Price += price_delta * (CustomerManager.GetMaxTTL() / ttl);
            }
            existingProd.Invest_tend += product.amount / 2 + 1;

        }
        else // product does not exist in store
        {
            int ttl = cc.ttl; // this is used for a race condition
            if (ttl == 0)
                ttl = 1;
            product.Price += price_delta * (CustomerManager.GetMaxTTL() / ttl);
            if (existingProd != null)
                existingProd.Invest_tend += product.amount / 2; // buy half of wanted
        }
    }

    public List<int> GetSoldProducts()
    {
        List<int> sold = soldProducts.Values.Select(val => val).ToList();
        //soldProducts.Clear();
        return sold;
    }

    public void ClearSoldProducts()
    {
        soldProducts.Keys.ToList().ForEach(p => soldProducts[p] = 0);
    }

    public int GetQueueSize()
    {
        return queue.Count;
    }

    public void OnDrawGizmosSelected()
    {
        PrintShop();
    }

    // MLAgents logic
    public void UpdateFromModelOutput(List<float> deltas)
    {
        for (int i = 0; i < products.Count; i++)
        {
            Product p = products.Values.ToList()[i];
            p.Price += deltas[i];
            p.Invest_tend += (int)deltas[i + 5];
        }
        float prob = deltas[10];

        if (prob > 0.5 && level < 3 && balance > 0)
        {
            SetLevel(++level, true);
        }
    }
}
