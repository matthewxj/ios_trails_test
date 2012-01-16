using UnityEngine;
using System.Collections;

public class TrailDetection : MonoBehaviour {
	
	public GUISkin mySkin;
	public GameObject knife;
	public float straightLineErrorRange=15;
	public float circleAngleErrorRange=30;
	
    private GameObject prefab;
	private bool switcher;

	string statusText= "";
	Rect statusTextRect;
	ArrayList xArray;
	ArrayList yArray;
	
	enum LineDirection {Vertical, Horizontal};
	enum Trends {goUp, goDown, goLeft, goRight, Stay};
	
	// Use this for initialization
	void Start () {
		statusText = "Start testing...";
		statusTextRect = new Rect( 10, 10, 300, 300 );
		
		xArray=new ArrayList();
		yArray=new ArrayList();
	}
	
	// Update is called once per frame
	void Update () {
		if (switcher) {
			Vector3 curScreenSpace= new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 10); 
			Vector3 curPosition= Camera.main.ScreenToWorldPoint(curScreenSpace);
			prefab.transform.position= curPosition;
	
			xArray.Add(prefab.transform.position.x);
			yArray.Add(prefab.transform.position.y);
		}
		
	    if(Input.touchCount > 0)  { 
	    
			if(Input.GetTouch(0).phase==TouchPhase.Began) {
				switcher = true;
				Vector3 startScreenSpace= new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 10); 
				Vector3 startPosition= Camera.main.ScreenToWorldPoint(startScreenSpace);
				prefab = (GameObject)Instantiate(knife, startPosition, Quaternion.identity);
	
				xArray.Add(prefab.transform.position.x);
				yArray.Add(prefab.transform.position.y);
				statusText = " Touching...";
			}
	
			if(Input.GetTouch(0).phase==TouchPhase.Ended) {
				switcher = false;
				Destroy(prefab);
				statusText = " Touching End";
				
				DetectShape();
			}
		}
	}

	void  DetectShape () {
		//Debug.Log(xArray);
		//Debug.Log(yArray);
		
		int lineType = DetectLineType ();
		
		if(lineType==1) { 
			statusText = " You draw a Vertical line";
		} else if(lineType==2) {
			statusText = " You draw a Horizontal line";			
		} else if(lineType==3) {
			statusText = " You draw a squiggle";	
		} else {
			statusText = " You draw a curve";
			
			if(IsCircle()){
				statusText = " You draw a circle";
			}
		}	
		
		xArray.Clear();
		yArray.Clear();
	}
	
	/***************
	*	lineTpyes
	*	0: not a line
	*	1: Vertical line
	*	2: Horizontal line
	*	3: Squiggle 
	****************/
	int DetectLineType () {
		
		float x1 = (float)xArray[0];
		float y1 = (float)yArray[0];
		float x2 = (float)xArray[xArray.Count-1];
		float y2 = (float)yArray[yArray.Count-1];
		
		//Debug.Log(x1+" "+y1+" "+x2+" "+y2);
		
		float a=y2-y1;
		float b=x1-x2;
		float c=(x2*y1)-(x1*y2);
		float sqrtAB= Mathf.Sqrt(a*a+b*b);
		
		LineDirection direction;
		
		if(Mathf.Abs(a)>Mathf.Abs(b)) {
			direction = LineDirection.Vertical;
		} else {
			direction = LineDirection.Horizontal;
		}
		
		int count = 0;
		Trends pastTrend=Trends.Stay;
		int trendChanges=0;

		for(int i=1;i<=xArray.Count-2;i++) {
			if((xArray[i] != null) && (yArray[i] != null)) {							
				float x0 = (float)xArray[i];
				float y0 = (float)yArray[i];
				float distance = Mathf.Abs(a*x0+b*y0+c);
				distance = distance / sqrtAB;
					//Debug.Log(distance);
				if(distance >= straightLineErrorRange) {
					count++;
				}
				
				if(count>0) {
					
					if(direction == LineDirection.Vertical) {
						if(IsinRange(y1,y2,y0)){
							
							Trends curTrend=Trends.Stay;
							
							if((float)xArray[i] > (float)xArray[i-1]) {
								curTrend = Trends.goRight;
							} else if ((float)xArray[i] < (float)xArray[i-1]) {
								curTrend = Trends.goLeft;
							}
							
							if(curTrend != Trends.Stay && pastTrend == Trends.Stay) {
								pastTrend = curTrend;
							}
							
							if(curTrend != Trends.Stay && pastTrend != Trends.Stay && curTrend != pastTrend) {
								trendChanges++;
								pastTrend = curTrend;
							}					
							
						} else {
							return 0;
						}
					} else {
						if(IsinRange(x1,x2,x0)){
													
							Trends curTrend=Trends.Stay;
							
							if((float)yArray[i] > (float)yArray[i-1]) {
								curTrend = Trends.goUp;
							} else if ((float)yArray[i] < (float)yArray[i-1]) {
								curTrend = Trends.goDown;
							}
							
							if(curTrend != Trends.Stay && pastTrend == Trends.Stay) {
								pastTrend = curTrend;
							}
							
							if(curTrend != Trends.Stay && pastTrend != Trends.Stay && curTrend != pastTrend) {
								trendChanges++;
								pastTrend = curTrend;
							}					
							
						} else {
							return 0;
						}
					}
				}
			}
		}
		
		//Debug.Log(count);
		
		if(count<1) {
			if(direction == LineDirection.Vertical) {
				return 1;			
			} else {				
				return 2;
			}		
		} 
		
		if(trendChanges>=2) {
			return 3;
		}
		
		return 0;
	}
	
	bool IsCircle () {
	
		float angleSum=0;
		int i = 0;
		while ((angleSum<=360)&&(i<=xArray.Count-3)) {
			
			float x1=0.0f;
			float y1=0.0f;
			float x2=0.0f;
			float y2=0.0f;
			float x3=0.0f;
			float y3=0.0f;
			
			if((xArray[i] != null) && (yArray[i] != null)) {
				x1 = (float)xArray[i];
				y1 = (float)yArray[i];
			}
			//Debug.Log("i="+i+" p1="+x1+" "+y1);
			
			i++;
			if((xArray[i] != null) && (yArray[i] != null)) {
				x2 = (float)xArray[i];
				y2 = (float)yArray[i];
			}
			//Debug.Log("i="+i+" p2="+x2+" "+y2);		
			while((i<=xArray.Count-2)&&(Distance(x1,y1,x2,y2)<1)) {
				i++;
				if((xArray[i] != null) && (yArray[i] != null)) {
					x2 = (float)xArray[i];
					y2 = (float)yArray[i];
				}
				//Debug.Log("i="+i+" p2="+x2+" "+y2);
			}		
			if(i>xArray.Count-2) {
				break;
			}
			
			i++;	
			if((xArray[i] != null) && (yArray[i] != null)) {
				x3 = (float)xArray[i];
				y3 = (float)yArray[i];
			}
			//Debug.Log("i="+i+" p3="+x3+" "+y3);
			while((i<=xArray.Count-1)&&((Distance(x2,y2,x3,y3)<1)||(IsSameLine(x1,y1,x2,y2,x3,y3)))) {
				i++;
				if((xArray[i] != null) && (yArray[i] != null)) {
					x3 = (float)xArray[i];
					y3 = (float)yArray[i];
				}
				//Debug.Log("i="+i+" p3="+x3+" "+y3);	
			}		
			if(i>xArray.Count-1) {
				break;
			}
			
			float x0 = ((y3 - y1) * (y2 * y2 - y1 * y1 + x2 * x2 - x1 * x1) + (y2 - y1) * (y1 * y1 - y3 * y3 + x1 * x1 - x3 * x3)) / (2 * (x2 - x1) * (y3 - y1) - 2 * (x3 - x1) * (y2 - y1));
	    	float y0 = ((x3 - x1) * (x2 * x2 - x1 * x1 + y2 * y2 - y1 * y1) + (x2 - x1) * (x1 * x1 - x3 * x3 + y1 * y1 - y3 * y3)) / (2 * (y2 - y1) * (x3 - x1) - 2 * (y3 - y1) * (x2 - x1));
	    	float r = Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
			
			//Debug.Log(x1+" "+y1+" "+x2+" "+y2+" "+x3+" "+y3);
			//Debug.Log(x0+" "+y0);	
			//Debug.Log(r);
			
			float d = Distance(x1,y1,x3,y3);
			//Debug.Log(d);
			
			float angle = Mathf.Asin(d/(2*r))*2*180/Mathf.PI;			
			angleSum +=angle;
			
			//Debug.Log("r="+r+" d="+d+" angle="+angle+" sum="+angleSum+" i="+i);
		}
		
		if(angleSum>(360-circleAngleErrorRange)) {
			return true;
		}
		
		return false;
	}
	
	bool IsinRange (float r1, float r2, float s) {
		float min = (r1<r2) ? r1:r2;
		float max = (r1>r2) ? r1:r2;
		
		if(s>=min && s<=max) {
			return true;
		} else {
			return false;
		}
	}
	
	float Distance ( float x1 ,  float y1 ,  float x2 ,  float y2  ) {
		float dist = Mathf.Sqrt((x2-x1)*(x2-x1)+(y2-y1)*(y2-y1));
		return dist;
	}
	
	bool IsSameLine ( float x1 ,  float y1 ,  float x2 ,  float y2 ,  float x3 ,  float y3  ){
		float k1=(y2-y1)/(x2-x1);
		float k2=(y3-y1)/(x3-x1);
		//Debug.Log("k1="+k1+" k2="+k2);
		
		if(Mathf.Abs(k2-k1)<0.01f) {
			return true;
		} else {
			return false;
		}
	}
	
	void  OnGUI (){
		GUI.skin = mySkin;
		GUI.Label( statusTextRect, statusText );
	}

}
