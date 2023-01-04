using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*This module contatins all of the simualtor params*/


/*Customer Params*/
public class CustomerParams
{
    public int CUSTOMER_COUNT = 50;
    public int SHOPPING_LIST_PRODUCT_AMOUNT_MAX = 5;
    public int SHOPPING_LIST_PRODUCT_AMOUNT_MIN = 1;
    public int TTL = 3;
    public int ALPHA = 15;
    public float CUSTOMER_SHOP_PROXIMITY = 0.02f;
}



/*Store Params*/
public class StoreParams
{
    public int MAX_SHOP_COUNT = 8;
    public int SHOP_STARTING_BALANCE = 250;
    public int BASE_TAX = 150;
    public int STOCK_LEVEL_ONE = 20;
    public int STOCK_LEVEL_TWO = 35;
    public int STOCK_LEVEL_THREE = 50;
    public int UPGRADE_LEVEL_TWO_PRICE = 1200;
    public int UPGRADE_LEVEL_THREE_PRICE = 8000;
    public int PRICE_OFFSET = 15;
    public float PRICE_DELTA_BASE = 0.05f;
    public float PRICE_DELTA_UPPER_BOUND = 0.05f;
    public float PRICE_DELTA_LOWER_BOUND = -0.03f;
    public float BANKRUPT_UPPER_BOUND = 0.02f;
    public float BANKRUPT_LOWER_BOUND = 0f;

}


/*Market Params*/
public class MarketParams
{
    public bool VISUAL_TRADING_MODE = true;
}