using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*This module contatins all of the simualtor params*/


/*Customer Params*/
public abstract class CustomerParams
{
    public static int CUSTOMER_COUNT = 40;
    public static int SHOPPING_LIST_PRODUCT_AMOUNT_MAX = 5;
    public static int SHOPPING_LIST_PRODUCT_AMOUNT_MIN = 1;
    public static int TTL = 3;
    public static int VISIT_COUNT = 3;
    public static int ALPHA = 15;
    public static float CUSTOMER_WAYPOINT_PROXIMITY = 0.03f;
}

/*Store Params*/
public abstract class StoreParams
{
    public static int MAX_STORE_COUNT = 9;
    public static int STORE_STARTING_BALANCE = 500;
    public static int BASE_TAX = 50;
    public static int STOCK_LEVEL_ONE = 20;
    public static int STOCK_LEVEL_TWO = 35;
    public static int STOCK_LEVEL_THREE = 50;
    public static int UPGRADE_LEVEL_TWO_PRICE = 1200;
    public static int UPGRADE_LEVEL_THREE_PRICE = 8000;
    public static int PRICE_OFFSET = 15;
    public static float PRICE_DELTA_BASE = 0.05f;
    public static float PRICE_DELTA_UPPER_BOUND = 0.05f;
    public static float PRICE_DELTA_LOWER_BOUND = -0.03f;
    public static float BANKRUPT_UPPER_BOUND = 0.02f;
    public static float BANKRUPT_LOWER_BOUND = 0f;
    public static float INITIAL_PRECENTILE = 1f / 2;
}

/*Market Params*/
public abstract class MarketParams
{
    public static bool VISUAL_TRADING_MODE = true;
}

/* MLAgents Params */
public abstract class MLParams
{
    public static int Transaction_Delta = 3;
    public static int Workdays = 3;
    public static int Phase = 45;
}