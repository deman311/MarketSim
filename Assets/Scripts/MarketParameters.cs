using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*This module contatins all of the simualtor params*/


/*Customer Params*/
public static class CustomerParams
{
    public static int CUSTOMER_COUNT = 5;
    public static int TTL = 3;
    public static float ALPHA = 15;
}



/*Store Params*/
public static class StoreParams
{
    public static int SHOP_COUNT = 10;
    public static int SHOP_STARTING_BALANCE = 250;
    public static int BASE_TAX = 150;
    public static float GAMMA = 15;
    public static float ALPHA = 15;
    public static float BETA = 15;

}


/*Market Params*/
public static class MarketParams
{
    public static bool VISUAL_TRADING_MODE = true;
}
