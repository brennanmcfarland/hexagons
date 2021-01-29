using UnityEngine;
using System.Collections;

public class Firegolem_Script : Actor {

    //**************STATS*********************
    public override int MAXHEALTH { get { return 3; } }
    public override int NORMALSPEED { get { return 5; } }
    public override int NORMALATTACKSTRENGTH { get { return 2; } }
    public override int NORMALDEFENSESTRENGTH { get { return 5; } }
    public override int NORMALINITIATIVE { get { return 40; } }
    //state-dependent stats inherited from actor

    public override bool isdefaultattackranged { get { return false; } }
    public override float LOSOFFSETFROMORIGIN { get { return 1.15f; } }
    public override int[][] multipletiles { get { return null; } }
    public override Hashtable abilityranges
    {
        get
        {
            return new Hashtable()
            {
                {"flame",3},
                {"brand",1},
                {"explode",1},
            };
        }
    }

    public GameObject character;

    public void spawn(int[] coords) //coords[0] = x, coords[1] = y
    {
        health = MAXHEALTH;
        speed = NORMALSPEED;
        attackstrength = NORMALATTACKSTRENGTH;
        defensestrength = NORMALDEFENSESTRENGTH;
        initiative = NORMALINITIATIVE;
        defaultability = "flame";
        selectedability = defaultability;
        //character = Instantiate(Resources.Load (@"Characters/barrabbas"), this.transform.position, Quaternion.identity) as GameObject;

        gameObject.AddComponent<Renderer>();
        gameObject.GetComponent<Renderer>().material = Resources.Load(@"Materials/characters/firegolem", typeof(Material)) as Material;
        //character.transform.parent = this.transform;

        thisrigidbody = gameObject.AddComponent<Rigidbody>(); //add the rigidbody to optimize collisions
        thisrigidbody.isKinematic = true; //turn off physics

        mousecollider = gameObject.AddComponent<CapsuleCollider>();
        mousecollider.center = new Vector3(0f, .8f, 0f);
        mousecollider.radius = .3f;
        mousecollider.height = 1.6f;

        hardacoustics = true;

        actions = new string[] { "flame", "brand", "explode", "wildfire"};
    }

    //use flame attack
    public void flame(GameObject targetactor)
    {
        object[] prms = new object[2];
        prms[0] = 2;
        prms[1] = new attackmodifiers();
        ((attackmodifiers)prms[1]).burning = true;
        targetactor.SendMessage("defend", prms, SendMessageOptions.RequireReceiver);
    }

    //use brand attack
    public void brand(GameObject targetactor)
    {
        //melee attack, should mark in some way, make future fire attacks more effective?
        Debug.Log("still working on this part...");
    }

    //use explode attack
    public void explode(GameObject targetactor)
    {
        //small blast radius attack
        Debug.Log("still working on this part...");
    }

    //use special attack wildfire
    public void divebomb(GameObject targetactor)
    {
        Debug.Log("still working on this part...");
        //some sort of fire ring thing?
    }

    //renders test UI, for testing purposes only use Unity's full UI system for final product
    void OnGUI()
    {
        if (gameObject.GetComponent<PlayerActor>() != null && GameObject.FindWithTag("Player").GetComponent<ActorDriver>().characters[GameObject.FindWithTag("Player").GetComponent<ActorDriver>().activecharacter] == gameObject)
        {
            if (GUI.Button(new Rect(20, 40, 150, 20), "flame"))
                changeability("flame");
            if (GUI.Button(new Rect(20, 60, 150, 20), "brand"))
                changeability("brand");
            if (GUI.Button(new Rect(20, 80, 150, 20), "explode"))
                changeability("explode");
            if (GUI.Button(new Rect(20, 100, 150, 20), "revert to default"))
                cancelability();
            if (GUI.Button(new Rect(20, 120, 180, 20), "special: wildfire"))
                changeability("wildfire");
        }
    }

    //ranged attack is flame
    public override void rangedattack(GameObject targetactor)
    {
        Debug.Log("Using flame");
        SendMessage("flame", targetactor);
    }

    //must override these methods of the same names in actor class
    public override float getLOSoffsetfromorigin()
    {
        return LOSOFFSETFROMORIGIN;
    }
    public override int getspeed()
    {
        return speed;
    }
    public override int[][] getmultipletiles()
    {
        return multipletiles;
    }
    //serializes the character to a representative string
    //the actual serialization to a save file is handled by serialization
    public override savedata serializecharacter()
    {
        savedata temp = new savedata();
        if (this.gameObject.GetComponent<PlayerActor>())
            temp.isplayercharacter = true;
        else
            temp.isplayercharacter = false;
        int[] tempstats = { health, speed, attackstrength, defensestrength };
        int[] tempposition = { Xindex, Yindex };
        string[] tempmodifiers = { "unmodified" };
        temp.ID = this.GetType().ToString();
        temp.stats = tempstats;
        temp.position = tempposition;
        temp.modifiers = tempmodifiers;

        if (!temp.verifydata())
            Debug.Log("Error: Invalid save data for object " + this.GetType());
        return temp;
    }

    //deserializes the character once the class is created, instantiation is already taken care of elsewhere
    public override void deserializecharacter(savedata savestate)
    {
        if (savestate.verifydata() == false)
        {
            Debug.Log("Error loading character " + this.ToString());
            return;
        }
        health = savestate.stats[0];
        speed = savestate.stats[1];
        attackstrength = savestate.stats[2];
        defensestrength = savestate.stats[3];
        Debug.Log("Character " + this.ToString() + " successfully loaded");
    }
}
