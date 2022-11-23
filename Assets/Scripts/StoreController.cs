using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StoreController : MonoBehaviour
{
    [SerializeField] Canvas uiBalance;

    int level = 1;
    float balance = 200f;
    float timer = 0f;
    StockManager sm;

    Dictionary<string, Product> products = new Dictionary<string, Product>();
    List<CustomerController> queue = new List<CustomerController>();

    public static object _QueueLock = new object();

    [SerializeField] TextMeshProUGUI cash, apple, shirt, phone, gpu, rolex; // UI Amounts

    void Awake()
    {
        sm = GameObject.Find("SimulationController").GetComponent<StockManager>();
        InitPricesAndIT();
        UpdateUIPrices(); // inital price set
        phone.transform.parent.gameObject.SetActive(false);
        gpu.transform.parent.gameObject.SetActive(false);
        rolex.transform.parent.gameObject.SetActive(false);

        //PrintShop();
    }

    void Update()
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

        if (SafeDequeue(out CustomerController cc))
            Transaction(cc);
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

    public void LevelUp()
    {
        level++;
        transform.localScale *= 1.2f;
        if (level == 2)
        {
            GetComponent<Renderer>().material.color = Color.green;
            phone.transform.parent.gameObject.SetActive(true);
            gpu.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.red;
            rolex.transform.parent.gameObject.SetActive(true);
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
            sb.Append("[S]" + this.name + " ");
        foreach (Product p in products.Values)
            sb.Append(p + ", ");

        if (!string.IsNullOrEmpty(sb.ToString()))
            Debug.Log(sb.ToString());
    }

    public void Restock()
    {
        foreach (string prodName in sm.BuyList(level))
        {
            // 50% to restock if no such product in stock
            if (Random.Range(0, 2) == 0 && (!products.TryGetValue(prodName, out Product p) || p.amount == 0))
                BuyProduct(new Product(prodName).SetInvestmentTendency(Random.Range(1, 11)));
        }
    }

    private bool IsStockEmpty()
    {
        return products.ToListPooled().TrueForAll(kvp => kvp.Value.amount == 0);
    }

    public void Tax(int TAX)
    {
        balance -= TAX * level * level;
        if (balance < 0)
            GameObject.Find("SimulationController").GetComponent<PathfindingManager>().SafeRemove(gameObject);
    }

    public void Transaction(CustomerController cc)
    {
        foreach (Product p in cc.GetProducts())
            SellProduct(p, cc);
        cc.CompleteTransaction();
    }

    public void RandomOffsetPrice(Product product, int epsilon)
    {
        float price = sm.GetProductionPrice(product.name);
        float offset = sm.GetMaxPrice(product.name) * Random.Range(0, epsilon) / 100;
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
        foreach (string prodName in sm.BuyList(level))
            BuyProduct(new Product(prodName));
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
            product.amount = existingProd.Invest_tend;
            if (balance - existingProd.amount * price < 0)
                product.amount = Mathf.FloorToInt(balance / price);
            balance -= product.amount * price;
            existingProd.amount += product.amount;
        }
        else
        {
            product.Invest_tend = Random.Range(1, 11);
            RandomOffsetPrice(product, 10);
            product.amount = product.Invest_tend;
            if (balance - product.amount * price < 0)
                product.amount = Mathf.FloorToInt(balance / price);
            balance -= product.amount * price;
            products.Add(product.name, product);
        }
    }

    /// <summary>
    /// Takes in a product that its values represent:
    ///     name - product name.
    ///     amount - how many to sell.
    ///     price / invest_rate - change for a transaction
    ///  returns the amount sold.
    /// </summary>
    public void SellProduct(Product product, CustomerController cc)
    {
        int IT = Random.Range(1, 4);
        float price_delta =
            GameObject.Find("SimulationController").GetComponent<StockManager>().GetMaxPrice(product.name)
            * (0.1f + Random.Range(-0.05f, 0.05f)); // 10% + alpha(-5%,5%) of max price

        if (products.TryGetValue(product.name, out Product existingProd))
        {
            if (existingProd.amount == 0)   // none in stock
                return;

            int sold = 0;
            if (existingProd.amount < product.amount)
                sold = existingProd.amount;
            else
                sold = product.amount;

            if (existingProd.Price <= product.Price)
            {
                existingProd.Price += price_delta * sold;
                existingProd.Invest_tend += IT * sold;
                existingProd.amount -= sold;
                balance += sold * existingProd.Price;
            }
            else // could not sell
            {
                // Store changes
                float bankruptPanic = Mathf.Clamp(existingProd.Price / balance, 0, 0.2f * existingProd.Price); // can only range from 0 to 3x
                existingProd.Price -= (price_delta + 20 * bankruptPanic) * sold; // 10% + delta + (20% to epsilon)
                existingProd.Invest_tend -= IT * sold;

                // Customer changes
                product.Price -= price_delta * (CustomerManager.GetMaxTTL() / cc.ttl) * sold;
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        PrintShop();
    }
}
